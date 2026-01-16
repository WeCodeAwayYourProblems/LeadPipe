using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class CornRepository(
    PlumbingContext context,
    ILogger<CornRepository> logger)
    : PlumbingContextRepository<CornEntity, CornRepository>(context, logger), IRepository<CornEntity>
{
    protected override IQueryable<CornEntity> WithIncludes(IQueryable<CornEntity> q)
    {
        return q
            .Include(c => c.CustardCornLinks)
            .Include(c => c.SandCornLinks)
            .Include(c => c.CornCaliperLinks)
            .Include(c => c.CornPlumbingLinks);
    }
    public override async Task<Result<List<CornEntity>>> UpsertRangeAsync(
        List<CornEntity> entities,
        CancellationToken ct = default)
    {
        if (entities.Count == 0)
            return Result.Success(new List<CornEntity>());

        AssertNotString<CornEntity>(nameof(CornEntity.PhoneNumber));
        AssertNotString<CornEntity>(nameof(CornEntity.Date));
        AssertNotString<CornEntity>(nameof(CornEntity.UnixDate));

        // Deduplication
        List<CornEntity> uniqueEntities =
        [
            .. entities
                .GroupBy(e => (e.PhoneNumber, e.Date))
                .Select(g => g.Last())
        ];

        int batchSize = 200;
        const int minBatchSize = 1;
        int stagedCount = 0;
        int skipped = 0;
        const string tempTable = "temp_corn";

        try
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(ct);

            await _context.Database.ExecuteSqlRawAsync($"""
                CREATE TEMP TABLE IF NOT EXISTS {tempTable} (
                    {nameof(CornEntity.PhoneNumber)} INTEGER NOT NULL,
                    {nameof(CornEntity.Date)} TEXT NOT NULL,
                    {nameof(CornEntity.UnixDate)} INTEGER NOT NULL,
                    {nameof(CornEntity.Payload)} TEXT NOT NULL,
                    {nameof(CornEntity.MetaData)} TEXT NOT NULL,
                    {nameof(CornEntity.Source)} TEXT NOT NULL,
                    PRIMARY KEY ({nameof(CornEntity.PhoneNumber)}, {nameof(CornEntity.Date)})
                ) WITHOUT ROWID;
            """, ct);

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

                    if (batchSize < 200)
                        batchSize = Math.Min(batchSize * 2, 200);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "{Entity} batch insert failed (size={BatchSize}). Reducing batch size.",
                        nameof(CornEntity),
                        batchSize);

                    if (batchSize == minBatchSize)
                    {
                        skipped++;
                        index++;
                        batchSize = 100;
                    }
                    else
                    {
                        batchSize = Math.Max(minBatchSize, batchSize / 2);
                    }
                }
            }

            int updated = await _context.Database.ExecuteSqlRawAsync($"""
                UPDATE {TableNames.CornEntitiesName}
                SET
                    {nameof(CornEntity.UnixDate)} = t.{nameof(CornEntity.UnixDate)},
                    {nameof(CornEntity.Payload)} = t.{nameof(CornEntity.Payload)},
                    {nameof(CornEntity.MetaData)} = t.{nameof(CornEntity.MetaData)},
                    {nameof(CornEntity.Source)} = t.{nameof(CornEntity.Source)}
                FROM {tempTable} t
                WHERE t.{nameof(CornEntity.PhoneNumber)} = {TableNames.CornEntitiesName}.{nameof(CornEntity.PhoneNumber)}
                  AND t.Date = {TableNames.CornEntitiesName}.Date;
            """, ct);

            int inserted = await _context.Database.ExecuteSqlRawAsync($"""
                INSERT INTO {TableNames.CornEntitiesName}
                    ({nameof(CornEntity.PhoneNumber)}, {nameof(CornEntity.Date)}, {nameof(CornEntity.UnixDate)}, {nameof(CornEntity.Payload)}, {nameof(CornEntity.MetaData)}, {nameof(CornEntity.Source)})
                SELECT
                    t.{nameof(CornEntity.PhoneNumber)},
                    t.{nameof(CornEntity.Date)},
                    t.{nameof(CornEntity.UnixDate)},
                    t.{nameof(CornEntity.Payload)},
                    t.{nameof(CornEntity.MetaData)},
                    t.{nameof(CornEntity.Source)}
                FROM {tempTable} t
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM {TableNames.CornEntitiesName} c
                    WHERE c.{nameof(CornEntity.PhoneNumber)} = t.{nameof(CornEntity.PhoneNumber)}
                      AND c.{nameof(CornEntity.Date)} = t.{nameof(CornEntity.Date)}
                );
            """, ct);

            await _context.Database.ExecuteSqlRawAsync(
                $"DELETE FROM {tempTable};", ct);

            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Updated={Updated}, Inserted={Inserted}, Skipped={Skipped}",
                nameof(CornEntity),
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
            _logger.LogError(ex, "{Entity} upsert failed", nameof(CornEntity));
            return Result.Failure<List<CornEntity>>(ex.ToString());
        }

        // --------------------
        // Local helper
        // --------------------
        void InsertBatch(List<CornEntity> batch)
        {
            var sql = new StringBuilder($"INSERT INTO {tempTable} VALUES ");

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];

                sql.Append($"""
                (
                    {e.PhoneNumber},
                    '{e.Date:yyyy-MM-dd HH:mm:ss}',
                    {e.UnixDate},
                    '{Clean(e.Payload)}',
                    '{Clean(e.MetaData)}',
                    '{Clean(e.Source)}'
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
