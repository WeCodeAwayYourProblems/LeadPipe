using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class CustardRepository
    (
        PlumbingContext context,
        ILogger<CustardRepository> logger
    ) : PlumbingContextRepository<CustardEntity, CustardRepository>(context, logger), IRepository<CustardEntity>
{
    protected override IQueryable<CustardEntity> WithIncludes(IQueryable<CustardEntity> q)
    {
        return q
            .Include(q => q.SandEntities)
            .Include(q => q.CustardCaliperLinks)
            .Include(q => q.CustardCornLinks)
            .Include(q => q.CustardPlumbingLinks);
    }

    public override async Task<Result<List<CustardEntity>>> UpsertRangeAsync(List<CustardEntity> entities, CancellationToken ct = default)
    {
        if (entities.Count == 0)
            return Result.Success(new List<CustardEntity>());

        AssertNotString<CustardEntity>(nameof(CustardEntity.Id));
        AssertNotString<CustardEntity>(nameof(CustardEntity.Active));
        AssertNotString<CustardEntity>(nameof(CustardEntity.PhoneNumber));
        AssertNotString<CustardEntity>(nameof(CustardEntity.PhoneNumber2));
        AssertNotString<CustardEntity>(nameof(CustardEntity.Date));
        AssertNotString<CustardEntity>(nameof(CustardEntity.UnixDate));
        AssertNotString<CustardEntity>(nameof(CustardEntity.CancelDate));
        AssertNotString<CustardEntity>(nameof(CustardEntity.UnixCancelDate));

        // Deduplicate in-memory by Id
        List<CustardEntity> uniqueEntities =
        [
            .. entities
            .GroupBy(e => e.Id)
            .Select(g => g.Last())
        ];

        int batchSize = 50;
        const int minBatchSize = 1;
        int stagedCount = 0;
        int skipped = 0;
        const string tempTable = "temp_custard_entities";

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            // Temp table
            await _context.Database.ExecuteSqlRawAsync($"""
            CREATE TEMP TABLE IF NOT EXISTS {tempTable} (
                {nameof(CustardEntity.Id)} INTEGER PRIMARY KEY,
                {nameof(CustardEntity.Active)} INTEGER,
                {nameof(CustardEntity.PhoneNumber)} INTEGER,
                {nameof(CustardEntity.PhoneNumber2)} INTEGER,
                {nameof(CustardEntity.Date)} TEXT,
                {nameof(CustardEntity.UnixDate)} INTEGER,
                {nameof(CustardEntity.CancelDate)} TEXT,
                {nameof(CustardEntity.UnixCancelDate)} INTEGER
            ) WITHOUT ROWID;
        """, ct);

            int index = 0;

            while (index < uniqueEntities.Count)
            {
                ct.ThrowIfCancellationRequested();

                int take = Math.Min(batchSize, uniqueEntities.Count - index);
                var batch = uniqueEntities.GetRange(index, take);

                try
                {
                    InsertBatch(batch);
                    stagedCount += batch.Count;
                    index += take;

                    if (batchSize < 50)
                        batchSize = Math.Min(batchSize * 2, 50);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "{Entity} batch insert failed (size={BatchSize}). Reducing batch size. Exception Message: {Message}",
                        nameof(CustardEntity),
                        batchSize,
                        ex.Message);

                    if (batchSize == minBatchSize)
                    {
                        var row = batch[0];
                        _logger.LogError(
                            "{Entity} row insert failed: Id={Id}",
                            nameof(CustardEntity),
                            row.Id);

                        index++;
                        batchSize = 25;
                        skipped++;
                    }
                    else
                    {
                        batchSize = Math.Max(minBatchSize, batchSize / 2);
                    }
                }
            }

            // ---- UPDATE existing ----
            int updated = await _context.Database.ExecuteSqlRawAsync($"""
            UPDATE {TableNames.CustardEntitiesName}
            SET
                {nameof(CustardEntity.Active)} = t.{nameof(CustardEntity.Active)},
                {nameof(CustardEntity.PhoneNumber)} = t.{nameof(CustardEntity.PhoneNumber)},
                {nameof(CustardEntity.PhoneNumber2)} = t.{nameof(CustardEntity.PhoneNumber2)},
                {nameof(CustardEntity.Date)} = t.{nameof(CustardEntity.Date)},
                {nameof(CustardEntity.UnixDate)} = t.{nameof(CustardEntity.UnixDate)},
                {nameof(CustardEntity.CancelDate)} = t.{nameof(CustardEntity.CancelDate)},
                {nameof(CustardEntity.UnixCancelDate)} = t.{nameof(CustardEntity.UnixCancelDate)}
            FROM {tempTable} t
            WHERE t.{nameof(CustardEntity.Id)} = {TableNames.CustardEntitiesName}.{nameof(CustardEntity.Id)};
        """, ct);

            // ---- INSERT missing ----
            int inserted = await _context.Database.ExecuteSqlRawAsync($"""
            INSERT INTO {TableNames.CustardEntitiesName} (
                {nameof(CustardEntity.Id)},
                {nameof(CustardEntity.Active)},
                {nameof(CustardEntity.PhoneNumber)},
                {nameof(CustardEntity.PhoneNumber2)},
                {nameof(CustardEntity.Date)},
                {nameof(CustardEntity.UnixDate)},
                {nameof(CustardEntity.CancelDate)},
                {nameof(CustardEntity.UnixCancelDate)}
            )
            SELECT
                {nameof(CustardEntity.Id)},
                {nameof(CustardEntity.Active)},
                {nameof(CustardEntity.PhoneNumber)},
                {nameof(CustardEntity.PhoneNumber2)},
                {nameof(CustardEntity.Date)},
                {nameof(CustardEntity.UnixDate)},
                {nameof(CustardEntity.CancelDate)},
                {nameof(CustardEntity.UnixCancelDate)}
            FROM {tempTable} t
            WHERE NOT EXISTS (
                SELECT 1
                FROM {TableNames.CustardEntitiesName} c
                WHERE c.{nameof(CustardEntity.Id)} = t.{nameof(CustardEntity.Id)}
            );
        """, ct);

            await _context.Database.ExecuteSqlRawAsync($"DELETE FROM {tempTable};", ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Updated={Updated}, Inserted={Inserted}, Skipped={Skipped}",
                nameof(CustardEntity),
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
            _logger.LogError(ex, "{Entity} upsert failed. Exception Message: {Message}", nameof(CustardEntity), ex.Message);
            return Result.Failure<List<CustardEntity>>(ex.ToString());
        }

        void InsertBatch(List<CustardEntity> batch)
        {
            var sql = new StringBuilder();
            sql.Append($"INSERT INTO {tempTable} VALUES ");

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];
                sql.Append('(')
                   .Append($"{e.Id}, ")
                   .Append($"{(e.Active ? 1 : 0)}, ")
                   .Append($"{e.PhoneNumber}, ")
                   .Append($"{e.PhoneNumber2}, ")
                   .Append($"'{e.Date:yyyy-MM-dd HH:mm:ss}', ")
                   .Append($"{e.UnixDate}, ")
                   .Append($"'{e.CancelDate:yyyy-MM-dd HH:mm:ss}', ")
                   .Append($"{e.UnixCancelDate}")
                   .Append(')');

                if (i < batch.Count - 1)
                    sql.Append(", ");
            }

            sql.Append(';');
            _context.Database.ExecuteSqlRaw(sql.ToString());
        }
    }

}
