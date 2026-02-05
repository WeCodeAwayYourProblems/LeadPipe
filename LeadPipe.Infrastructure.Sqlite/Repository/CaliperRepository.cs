using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class CaliperRepository
    (
        PlumbingContext context,
        ILogger<CaliperRepository> logger
    ) : PlumbingContextRepository<CaliperEntity, CaliperRepository>(context, logger), IRepository<CaliperEntity>
{
    protected override IQueryable<CaliperEntity> WithIncludes(IQueryable<CaliperEntity> q)
    {
        return q
            .Include(c => c.CustardCaliperLinks)
            .Include(c => c.SandCaliperLinks)
            .Include(c => c.PlumbingCaliperLinks)
            .Include(c => c.CornCaliperLinks);
    }

    public override async Task<Result<List<CaliperEntity>>> UpsertRangeAsync(
        List<CaliperEntity> entities,
        CancellationToken ct = default)
    {
        if (entities.Count == 0)
            return Result.Success(new List<CaliperEntity>());

        AssertNotString<CaliperEntity>(nameof(CaliperEntity.PhoneNumber));
        AssertNotString<CaliperEntity>(nameof(CaliperEntity.Date));
        AssertNotString<CaliperEntity>(nameof(CaliperEntity.UnixDate));
        AssertNotString<CaliperEntity>(nameof(CaliperEntity.Duration));

        int batchSize = 200;
        const int minBatchSize = 1;
        int stagedCount = 0;
        int skipped = 0;
        const string tempTable = "temp_caliper";

        try
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(ct);

            await _context.Database.ExecuteSqlRawAsync($"""
                CREATE TEMP TABLE IF NOT EXISTS {tempTable} (
                    {nameof(CaliperEntity.PhoneNumber)} INTEGER NOT NULL,
                    {nameof(CaliperEntity.Date)} TEXT NOT NULL,
                    {nameof(CaliperEntity.UnixDate)} INTEGER NOT NULL,
                    {nameof(CaliperEntity.Note)} TEXT,
                    {nameof(CaliperEntity.Source)} TEXT,
                    {nameof(CaliperEntity.Location)} TEXT,
                    {nameof(CaliperEntity.Duration)} INTEGER,
                    {nameof(CaliperEntity.Billable)} INTEGER,
                    PRIMARY KEY ({nameof(CaliperEntity.PhoneNumber)}, {nameof(CaliperEntity.Date)})
                ) WITHOUT ROWID;
            """, ct);

            int index = 0;

            while (index < entities.Count)
            {
                int take = Math.Min(batchSize, entities.Count - index);
                var batch = entities.GetRange(index, take);

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
                        nameof(CaliperEntity),
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
                UPDATE {TableNames.CaliperEntitiesName}
                SET
                    {nameof(CaliperEntity.UnixDate)} = t.{nameof(CaliperEntity.UnixDate)},
                    {nameof(CaliperEntity.Note)} = t.{nameof(CaliperEntity.Note)},
                    {nameof(CaliperEntity.Source)} = t.{nameof(CaliperEntity.Source)},
                    {nameof(CaliperEntity.Location)} = t.{nameof(CaliperEntity.Location)},
                    {nameof(CaliperEntity.Duration)} = t.{nameof(CaliperEntity.Duration)},
                    {nameof(CaliperEntity.Billable)} = t.{nameof(CaliperEntity.Billable)}
                FROM {tempTable} t
                WHERE t.{nameof(CaliperEntity.PhoneNumber)} = {TableNames.CaliperEntitiesName}.{nameof(CaliperEntity.PhoneNumber)}
                  AND t.{nameof(CaliperEntity.Date)} = {TableNames.CaliperEntitiesName}.{nameof(CaliperEntity.Date)};
            """, ct);

            int inserted = await _context.Database.ExecuteSqlRawAsync($"""
                INSERT INTO {TableNames.CaliperEntitiesName}
                    ({nameof(CaliperEntity.PhoneNumber)}, {nameof(CaliperEntity.Date)}, {nameof(CaliperEntity.UnixDate)}, {nameof(CaliperEntity.Note)}, {nameof(CaliperEntity.Source)}, {nameof(CaliperEntity.Location)}, {nameof(CaliperEntity.Duration)}, {nameof(CaliperEntity.Billable)})
                SELECT
                    t.{nameof(CaliperEntity.PhoneNumber)},
                    t.{nameof(CaliperEntity.Date)},
                    t.{nameof(CaliperEntity.UnixDate)},
                    t.{nameof(CaliperEntity.Note)},
                    t.{nameof(CaliperEntity.Source)},
                    t.{nameof(CaliperEntity.Location)},
                    t.{nameof(CaliperEntity.Duration)},
                    t.{nameof(CaliperEntity.Billable)}
                FROM {tempTable} t
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM {TableNames.CaliperEntitiesName} c
                    WHERE c.{nameof(CaliperEntity.PhoneNumber)} = t.{nameof(CaliperEntity.PhoneNumber)}
                      AND c.{nameof(CaliperEntity.Date)} = t.{nameof(CaliperEntity.Date)}
                );
            """, ct);

            await _context.Database.ExecuteSqlRawAsync(
                $"DELETE FROM {tempTable};", ct);

            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Staged={Staged}, Updated={Updated}, Inserted={Inserted}, Skipped={Skipped}",
                nameof(CaliperEntity),
                entities.Count,
                stagedCount,
                updated,
                inserted,
                skipped);

            return Result.Success(entities);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Entity} upsert failed. Exception Message: {Message}", nameof(CaliperEntity), ex.Message);
            return Result.Failure<List<CaliperEntity>>(ex.ToString());
        }

        // --------------------
        // Local helper
        // --------------------
        void InsertBatch(List<CaliperEntity> batch)
        {
            var sql = new StringBuilder($"INSERT INTO {tempTable} VALUES ");

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];
                sql.Append('(')
                    .Append(e.PhoneNumber).Append(',')
                    .Append($"'{e.Date:yyyy-MM-dd HH:mm:ss}',")
                    .Append($"{e.UnixDate},")
                    .Append($"'{Clean(e.Note)}',")
                    .Append($"'{Clean(e.Source)}',")
                    .Append($"'{Clean(e.Location)}',")
                    .Append($"{e.Duration},")
                    .Append($"{(e.Billable ? 1 : 0)}")
                    .Append(')');
                sql.AppendLine();


                if (i < batch.Count - 1)
                    sql.Append(", ");
            }

            sql.Append(';');
            _context.Database.ExecuteSqlRaw(sql.ToString());
        }
    }
}
