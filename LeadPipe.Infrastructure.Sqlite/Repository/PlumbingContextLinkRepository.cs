using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public abstract class PlumbingContextLinkRepository<TEntity, TRepo>
    (
        PlumbingContext context,
        ILogger<TRepo> logger
    ) : PlumbingContextBaseRepository<TEntity, TRepo>(context, logger)
    where TEntity : class, IEntity
{
    protected record UpsertFields(string TableName, string TempTable, string Id1, string Id2, string PhoneCol, string DateCol, string EntityName);
    protected abstract Task AddLinks(List<TEntity> links, int batchSize, CancellationToken ct);
    protected abstract UpsertFields LinkDetails { get; }
    protected string TempId1 = "id1";
    protected string TempId2 = "id2";
    protected string TempPhone = "phone";
    protected string TempDate = "matchDate";
    protected virtual string TempTable => $"""
        DROP TABLE IF EXISTS {LinkDetails.TempTable};
        CREATE TEMP TABLE {LinkDetails.TempTable} (
            {TempId1} INTEGER,
            {TempId2} INTEGER,
            {TempPhone} INTEGER,
            {TempDate} INTEGER
        );
    """;
    protected virtual string UpdateSql => $"""
        UPDATE {LinkDetails.TableName}
        SET {LinkDetails.PhoneCol} = (
                SELECT t.{TempPhone}
                FROM {LinkDetails.TempTable} t
                WHERE t.{TempId1} = {LinkDetails.TableName}.{LinkDetails.Id1}
                  AND t.{TempId2} = {LinkDetails.TableName}.{LinkDetails.Id2}
                  AND t.{TempPhone} <> 0
                  AND t.{TempDate} < {LinkDetails.TableName}.{LinkDetails.DateCol}
                ORDER BY t.{TempDate} ASC
                LIMIT 1
            ),
            {LinkDetails.DateCol} = (
                SELECT t.{TempDate}
                FROM {LinkDetails.TempTable} t
                WHERE t.{TempId1} = {LinkDetails.TableName}.{LinkDetails.Id1}
                  AND t.{TempId2} = {LinkDetails.TableName}.{LinkDetails.Id2}
                  AND t.{TempPhone} <> 0
                  AND t.{TempDate} < {LinkDetails.TableName}.{LinkDetails.DateCol}
                ORDER BY t.{TempDate} ASC
                LIMIT 1
            )
        WHERE EXISTS (
            SELECT 1
            FROM {LinkDetails.TempTable} t
            WHERE t.{TempId1} = {LinkDetails.TableName}.{LinkDetails.Id1}
              AND t.{TempId2} = {LinkDetails.TableName}.{LinkDetails.Id2}
              AND t.{TempPhone} <> 0
              AND t.{TempDate} < {LinkDetails.TableName}.{LinkDetails.DateCol}
        );
    """;
    protected virtual string InsertSql => $"""
        INSERT INTO {LinkDetails.TableName} 
        (
            {LinkDetails.Id1}, 
            {LinkDetails.Id2}, 
            {LinkDetails.PhoneCol}, 
            {LinkDetails.DateCol}
        )
        SELECT 
            t.{TempId1}, 
            t.{TempId2},
            (
                SELECT t2.{TempPhone}
                FROM {LinkDetails.TempTable} t2
                WHERE t2.{TempId1} = t.{TempId1}
                  AND t2.{TempId2} = t.{TempId2}
                  AND t2.{TempPhone} <> 0
                ORDER BY t2.{TempDate} ASC
                LIMIT 1
            ),
            MIN(t.{TempDate})
        FROM {LinkDetails.TempTable} t
        WHERE t.{TempPhone} <> 0
          AND NOT EXISTS (
              SELECT 1 
              FROM {LinkDetails.TableName} ccl
              WHERE ccl.{LinkDetails.Id1} = t.{TempId1}
                AND ccl.{LinkDetails.Id2} = t.{TempId2}
          )
        GROUP BY t.{TempId1}, t.{TempId2};
    """;

    internal async Task<Result<List<TEntity>>> UpsertLinkRangeAsync(
        List<TEntity> links,
        CancellationToken ct) 
    {
        if (links.Count == 0)
        {
            _logger.LogInformation(
                "{Entity} upsert reverted. No entities entered. Returning success state.",
                LinkDetails.EntityName
            );
            return Result.Success(links);
        }

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            // Create temp table with id1/id2
            await _context.Database.ExecuteSqlRawAsync(TempTable, ct);

            // Insert in batches
            int batchSize = 999 / 4;
            await AddLinks(links, batchSize, ct);

            // Update existing rows (earliest match date wins)
            int totalUpdated = await _context.Database.ExecuteSqlRawAsync(UpdateSql, ct);

            // Insert new rows
            int totalInserted = await _context.Database.ExecuteSqlRawAsync(InsertSql, ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Updated={Updated}, Inserted={Inserted}",
                LinkDetails.EntityName, links.Count, totalUpdated, totalInserted
            );

            await transaction.CommitAsync(ct);
            return Result.Success(links);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Entity} upsert failed for table {Table}", LinkDetails.EntityName, LinkDetails.TableName);
            return Result.Failure<List<TEntity>>(ex.ToString());
        }
    }

}