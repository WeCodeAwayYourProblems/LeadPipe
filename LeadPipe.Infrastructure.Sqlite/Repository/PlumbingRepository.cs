using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public class PlumbingRepository(PlumbingContext context, ILogger<PlumbingRepository> logger)
    : PlumbingContextRepository<PlumbingEntity, PlumbingRepository>(context, logger), IRepository<PlumbingEntity>
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

        // We take the FIRST chronological record for each unique touch
        List<PlumbingEntity> uniqueEntities =
        [
            .. entities
            .OrderBy(e => e.Date)
            .GroupBy(e => (e.PhoneNumber.Number, e.Date, e.Source))
            .Select(g => g.First())
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
                    PRIMARY KEY ({nameof(PlumbingEntity.PhoneNumber)}, {nameof(PlumbingEntity.Date)}, {nameof(PlumbingEntity.Source)})
                ) WITHOUT ROWID;
                DELETE FROM {tempTable};
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
                    _logger.LogError(
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

            int totalAffected = await _context.Database.ExecuteSqlRawAsync($"""
                INSERT INTO {TableNames.PlumbingEntitiesName} 
                (
                    {nameof(PlumbingEntity.PhoneNumber)},
                    {nameof(PlumbingEntity.Date)}, 
                    {nameof(PlumbingEntity.UnixDate)}, 
                    {nameof(PlumbingEntity.Contents)}, 
                    {nameof(PlumbingEntity.Source)}, 
                    {nameof(PlumbingEntity.MetaData)}
                )
                SELECT 
                    {nameof(PlumbingEntity.PhoneNumber)},
                    {nameof(PlumbingEntity.Date)},
                    {nameof(PlumbingEntity.UnixDate)},
                    {nameof(PlumbingEntity.Contents)},
                    {nameof(PlumbingEntity.Source)},
                    {nameof(PlumbingEntity.MetaData)}
                FROM {tempTable}
                ON CONFLICT({nameof(PlumbingEntity.PhoneNumber)}, {nameof(PlumbingEntity.Date)}, {nameof(PlumbingEntity.Source)}) DO UPDATE SET
                    {nameof(PlumbingEntity.UnixDate)} = excluded.{nameof(PlumbingEntity.UnixDate)},
                    {nameof(PlumbingEntity.Contents)} = excluded.{nameof(PlumbingEntity.Contents)},
                    {nameof(PlumbingEntity.MetaData)} = excluded.{nameof(PlumbingEntity.MetaData)};
            """, ct);

            await _context.Database.ExecuteSqlRawAsync($"DELETE FROM {tempTable};", cancellationToken: ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Staged={Staged}, Affected={Affected}, Skipped={Skipped}",
                nameof(PlumbingEntity),
                entities.Count,
                stagedCount,
                totalAffected,
                skipped);

            return Result.Success(entities);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Entity} upsert failed.",
                nameof(PlumbingEntity));
            return Result.Failure<List<PlumbingEntity>>(ex.ToString());
        }

        void InsertBatch(List<PlumbingEntity> batch)
        {
            var values = new List<object>();
            var rows = new List<string>();
            const int cols = 6;

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];
                int o = i * cols;
                rows.Add($"({{{o}}},{{{o + 1}}},{{{o + 2}}},{{{o + 3}}},{{{o + 4}}},{{{o + 5}}})");

                values.Add(e.PhoneNumber.Number);
                values.Add(e.Date.ToString("yyyy-MM-dd HH:mm:ss"));
                values.Add(e.UnixDate);
                values.Add(e.Contents ?? (object)DBNull.Value);
                values.Add(e.Source.ToString()); // Ensure Enum is passed as string
                values.Add(e.MetaData ?? string.Empty);
            }
            string joined = $"INSERT INTO {tempTable} VALUES {string.Join(",", rows)}";
            _context.Database.ExecuteSqlRaw(joined, [.. values]);
        }
    }

}
