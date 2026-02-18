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
            string createTemp = $"""
                DROP TABLE IF EXISTS {LinkDetails.TempTable};
                CREATE TEMP TABLE {LinkDetails.TempTable} (
                    id1 INTEGER,
                    id2 INTEGER,
                    phone INTEGER,
                    matchDate INTEGER
                );
            """;
            await _context.Database.ExecuteSqlRawAsync(createTemp, ct);

            // Insert in batches
            int batchSize = 999 / 4;
            await AddLinks(links, batchSize, ct);

            // Update existing rows (earliest match date wins)
            string updateSql = $"""
            UPDATE {LinkDetails.TableName}
            SET {LinkDetails.PhoneCol} = (
                    SELECT t.phone
                    FROM {LinkDetails.TempTable} t
                    WHERE t.id1 = {LinkDetails.TableName}.{LinkDetails.Id1}
                      AND t.id2 = {LinkDetails.TableName}.{LinkDetails.Id2}
                      AND t.phone <> 0
                      AND t.matchDate < {LinkDetails.TableName}.{LinkDetails.DateCol}
                    ORDER BY t.matchDate ASC
                    LIMIT 1
                ),
                {LinkDetails.DateCol} = (
                    SELECT t.matchDate
                    FROM {LinkDetails.TempTable} t
                    WHERE t.id1 = {LinkDetails.TableName}.{LinkDetails.Id1}
                      AND t.id2 = {LinkDetails.TableName}.{LinkDetails.Id2}
                      AND t.phone <> 0
                      AND t.matchDate < {LinkDetails.TableName}.{LinkDetails.DateCol}
                    ORDER BY t.matchDate ASC
                    LIMIT 1
                )
            WHERE EXISTS (
                SELECT 1
                FROM {LinkDetails.TempTable} t
                WHERE t.id1 = {LinkDetails.TableName}.{LinkDetails.Id1}
                  AND t.id2 = {LinkDetails.TableName}.{LinkDetails.Id2}
                  AND t.phone <> 0
                  AND t.matchDate < {LinkDetails.TableName}.{LinkDetails.DateCol}
            );
        """;

            int totalUpdated = await _context.Database.ExecuteSqlRawAsync(updateSql, ct);

            // Insert new rows
            string insertSql = $"""
            INSERT INTO {LinkDetails.TableName} 
            (
                {LinkDetails.Id1}, 
                {LinkDetails.Id2}, 
                {LinkDetails.PhoneCol}, 
                {LinkDetails.DateCol}
            )
            SELECT t.id1, t.id2, t.phone, MIN(t.matchDate)
            FROM {LinkDetails.TempTable} t
            WHERE t.phone <> 0
              AND NOT EXISTS (
                  SELECT 1 FROM {LinkDetails.TableName} ccl
                  WHERE ccl.{LinkDetails.Id1} = t.id1 AND ccl.{LinkDetails.Id2} = t.id2
              )
            GROUP BY t.id1, t.id2;
        """;

            int totalInserted = await _context.Database.ExecuteSqlRawAsync(insertSql, ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Updated={Updated}, Inserted={Inserted}",
                LinkDetails.EntityName, links.Count, totalUpdated, totalInserted
            );

            await transaction.CommitAsync(ct);
            return Result.Success(links);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Entity} upsert failed for table {Table}", LinkDetails.EntityName, LinkDetails.TableName);
            return Result.Failure<List<TEntity>>(ex.ToString());
        }
    }

}