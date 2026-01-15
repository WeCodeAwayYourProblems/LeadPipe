using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public class PlumbingRepository(PlumbingContext context, ILogger<PlumbingRepository> logger)
    : PlumbingContextRepository<PlumbingEntity, PlumbingRepository>(context, logger), IPlumbingRepository
{
    protected override IQueryable<PlumbingEntity> WithIncludes(IQueryable<PlumbingEntity> q)
    {
        return q
            .Include(c => c.CustardPlumbingLinks)
            .Include(c => c.SandPlumbingLinks)
            .Include(c => c.PlumbingCaliperLinks)
            .Include(c => c.CornPlumbingLinks);
    }

    public override async Task<Result<List<PlumbingEntity>>> UpsertRangeAsync(List<PlumbingEntity> entities, CancellationToken ct = default)
    {
        if (entities == null || entities.Count == 0)
            return Result.Success(new List<PlumbingEntity>());

        AssertNotString<PlumbingEntity>(nameof(PlumbingEntity.PhoneNumber));
        AssertNotString<PlumbingEntity>(nameof(PlumbingEntity.Date));
        AssertNotString<PlumbingEntity>(nameof(PlumbingEntity.UnixDate));
        AssertNotString<PlumbingEntity>(nameof(PlumbingEntity.Source));

        // Deduplicate in-memory by (PhoneNumber, Source)
        List<PlumbingEntity> uniqueEntities =
            [
                .. entities
                    .GroupBy(e => (e.PhoneNumber, e.Source))
                    .Select(g => g.MaxBy(e => e.Date)!)
            ];

        int batchSize = 200;
        const int minBatchSize = 1;
        int skipped = 0;
        int stagedCount = 0;
        const string tempTable = "temp_plumbings";

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            // Temp table for staging
            await _context.Database.ExecuteSqlRawAsync($"""
                CREATE TEMP TABLE IF NOT EXISTS {tempTable} (
                    {nameof(PlumbingEntity.PhoneNumber)} INTEGER NOT NULL,
                    {nameof(PlumbingEntity.Date)} TEXT NOT NULL,
                    {nameof(PlumbingEntity.UnixDate)} INTEGER NOT NULL,
                    {nameof(PlumbingEntity.Contents)} TEXT,
                    {nameof(PlumbingEntity.Source)} TEXT NOT NULL,
                    {nameof(PlumbingEntity.MetaData)} TEXT NOT NULL,
                    PRIMARY KEY ({nameof(PlumbingEntity.PhoneNumber)}, {nameof(PlumbingEntity.Source)})
                ) WITHOUT ROWID;
            """, cancellationToken: ct);

            int index = 0;

            while (index < uniqueEntities.Count)
            {
                int take = Math.Min(batchSize, uniqueEntities.Count - index);
                var batch = uniqueEntities.GetRange(index, take);

                try
                {
                    InsertBatch(batch);
                    stagedCount += batch.Count;
                    index += take;

                    // Gradually scale back up after success
                    if (batchSize < 200)
                        batchSize = Math.Min(batchSize * 2, 200);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "{Entity} batch insert failed (size={BatchSize}). Reducing batch size.",
                        nameof(PlumbingEntity),
                        batchSize);

                    if (batchSize == minBatchSize)
                    {
                        var row = batch[0];
                        _logger.LogError(
                            "{Entity} row insert failed: Phone={Phone}, Date={Date}, Contents={Contents}, Source={Source}, MetaData={MetaData}",
                            nameof(PlumbingEntity),
                            row.PhoneNumber,
                            row.Date,
                            row.Contents,
                            row.Source,
                            row.MetaData);

                        index++;
                        skipped++;
                        batchSize = 100;
                    }
                    else
                    {
                        batchSize = Math.Max(minBatchSize, batchSize / 2);
                    }
                }
            }

            // ---- Phase 1: UPDATE existing rows ----
            int updated = await _context.Database.ExecuteSqlRawAsync($"""
                UPDATE {TableNames.PlumbingEntitiesName}
                SET
                    {nameof(PlumbingEntity.Date)} = (
                        SELECT t.{nameof(PlumbingEntity.Date)} 
                        FROM {tempTable} t 
                        WHERE t.{nameof(PlumbingEntity.PhoneNumber)} = {TableNames.PlumbingEntitiesName}.{nameof(PlumbingEntity.PhoneNumber)} 
                            AND t.{nameof(PlumbingEntity.Source)} = {TableNames.PlumbingEntitiesName}.{nameof(PlumbingEntity.Source)}
                    ),
                    {nameof(PlumbingEntity.UnixDate)} = (
                        SELECT t.{nameof(PlumbingEntity.UnixDate)} 
                        FROM {tempTable} t 
                        WHERE t.{nameof(PlumbingEntity.PhoneNumber)} = {TableNames.PlumbingEntitiesName}.{nameof(PlumbingEntity.PhoneNumber)} 
                            AND t.{nameof(PlumbingEntity.Source)} = {TableNames.PlumbingEntitiesName}.{nameof(PlumbingEntity.Source)}
                    ),
                    Contents = (
                        SELECT t.Contents 
                        FROM {tempTable} t 
                        WHERE t.{nameof(PlumbingEntity.PhoneNumber)} = {TableNames.PlumbingEntitiesName}.{nameof(PlumbingEntity.PhoneNumber)} 
                            AND t.{nameof(PlumbingEntity.Source)} = {TableNames.PlumbingEntitiesName}.{nameof(PlumbingEntity.Source)}
                    ),
                    MetaData = (
                    SELECT t.MetaData 
                    FROM {tempTable} t 
                    WHERE t.{nameof(PlumbingEntity.PhoneNumber)} = {TableNames.PlumbingEntitiesName}.{nameof(PlumbingEntity.PhoneNumber)} 
                        AND t.{nameof(PlumbingEntity.Source)} = {TableNames.PlumbingEntitiesName}.{nameof(PlumbingEntity.Source)})
                WHERE EXISTS (
                    SELECT 1 
                    FROM {tempTable} t 
                    WHERE t.{nameof(PlumbingEntity.PhoneNumber)} = {TableNames.PlumbingEntitiesName}.{nameof(PlumbingEntity.PhoneNumber)} 
                        AND t.{nameof(PlumbingEntity.Source)} = {TableNames.PlumbingEntitiesName}.{nameof(PlumbingEntity.Source)}
                );
            """, cancellationToken: ct);

            // ---- Phase 2: INSERT missing rows ----
            int inserted = await _context.Database.ExecuteSqlRawAsync($"""
                INSERT INTO {TableNames.PlumbingEntitiesName}
                    ({nameof(PlumbingEntity.PhoneNumber)}, {nameof(PlumbingEntity.Date)}, {nameof(PlumbingEntity.UnixDate)}, {nameof(PlumbingEntity.Contents)}, {nameof(PlumbingEntity.Source)}, {nameof(PlumbingEntity.MetaData)})
                SELECT
                    t.{nameof(PlumbingEntity.PhoneNumber)},
                    t.{nameof(PlumbingEntity.Date)},
                    t.{nameof(PlumbingEntity.UnixDate)},
                    t.{nameof(PlumbingEntity.Contents)},
                    t.{nameof(PlumbingEntity.Source)},
                    t.{nameof(PlumbingEntity.MetaData)}
                FROM {tempTable} t
                WHERE NOT EXISTS (
                    SELECT 1 
                    FROM {TableNames.PlumbingEntitiesName} c 
                    WHERE c.{nameof(PlumbingEntity.PhoneNumber)} = t.{nameof(PlumbingEntity.PhoneNumber)} 
                        AND c.{nameof(PlumbingEntity.Source)} = t.{nameof(PlumbingEntity.Source)}
                );
            """, cancellationToken: ct);

            await _context.Database.ExecuteSqlRawAsync($"DELETE FROM {tempTable};", cancellationToken: ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Updated={Updated}, Inserted={Inserted}, Skipped={Skipped}",
                nameof(PlumbingEntity),
                entities.Count,
                uniqueEntities.Count,
                stagedCount,
                updated,
                inserted,
                skipped);

            return Result.Success(uniqueEntities);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Entity} upsert failed",
                nameof(PlumbingEntity));
            return Result.Failure<List<PlumbingEntity>>(ex.ToString());
        }

        void InsertBatch(List<PlumbingEntity> batch)
        {
            var sql = new StringBuilder();
            sql.Append($"INSERT INTO {tempTable} VALUES ");

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];
                sql.Append($"""
                    (
                        {e.PhoneNumber},
                        '{e.Date:yyyy-MM-dd HH:mm:ss}',
                        {e.UnixDate},
                        '{Clean(e.Contents)}',
                        '{e.Source}',
                        '{Clean(e.MetaData)}'
                    )
                """);

                if (i < batch.Count - 1)
                    sql.Append(", ");
            }

            sql.Append(';');
            _context.Database.ExecuteSqlRaw(sql.ToString());
        }
    }
}
