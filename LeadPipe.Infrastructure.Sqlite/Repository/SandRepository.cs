using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class SandRepository(PlumbingContext context, ILogger<SandRepository> logger)
    : PlumbingContextRepository<SandEntity, SandRepository>(context, logger), ISandRepository
{
    protected override IQueryable<SandEntity> WithIncludes(IQueryable<SandEntity> q)
    {
        return q
            .Include(c => c.CustardEntity)
            .Include(c => c.SandPlumbingLinks)
            .Include(c => c.SandCaliperLinks)
            .Include(c => c.SandCornLinks);
    }

    public override async Task<Result<List<SandEntity>>> UpsertRangeAsync(
        List<SandEntity> entities,
        CancellationToken ct = default)
    {
        if (entities.Count == 0)
            return Result.Success(new List<SandEntity>());

        AssertNotString<SandEntity>(nameof(SandEntity.Id));
        AssertNotString<SandEntity>(nameof(SandEntity.CustardId));
        AssertNotString<SandEntity>(nameof(SandEntity.Date));
        AssertNotString<SandEntity>(nameof(SandEntity.UnixDate));
        AssertNotString<SandEntity>(nameof(SandEntity.CancelDate));
        AssertNotString<SandEntity>(nameof(SandEntity.UnixCancelDate));
        AssertNotString<SandEntity>(nameof(SandEntity.Value));
        AssertNotString<SandEntity>(nameof(SandEntity.Seller));
        AssertNotString<SandEntity>(nameof(SandEntity.Seller2));
        AssertNotString<SandEntity>(nameof(SandEntity.Seller3));

        // Deduplicate in-memory by Id
        List<SandEntity> uniqueEntities =
            [
                .. entities
                    .GroupBy(e => e.Id)
                    .Select(g => g.Last())
            ];

        int batchSize = 50;
        const int minBatchSize = 1;
        int stagedCount = 0;
        int skipped = 0;
        const string tempTable = "temp_sand_entities";

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            // Create temp table
            await _context.Database.ExecuteSqlRawAsync($"""
            CREATE TEMP TABLE IF NOT EXISTS {tempTable} (
                {nameof(SandEntity.Id)} INTEGER PRIMARY KEY,
                {nameof(SandEntity.CustardId)} INTEGER NOT NULL,
                {nameof(SandEntity.Date)} TEXT,
                {nameof(SandEntity.UnixDate)} INTEGER,
                {nameof(SandEntity.CancelDate)} TEXT,
                {nameof(SandEntity.UnixCancelDate)} INTEGER,
                {nameof(SandEntity.Active)} INTEGER,
                {nameof(SandEntity.Complete)} INTEGER,
                {nameof(SandEntity.Value)} REAL,
                {nameof(SandEntity.Type)} TEXT,
                {nameof(SandEntity.Seller)} INTEGER,
                {nameof(SandEntity.Seller2)} INTEGER,
                {nameof(SandEntity.Seller3)} INTEGER,
                {nameof(SandEntity.Offerman)} TEXT NOT NULL
            ) WITHOUT ROWID;
            """, ct);

            int index = 0;

            // Batch insert into temp table
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
                    _logger.LogWarning(ex, "{Entity} batch insert failed (size={BatchSize}). Reducing batch size.", nameof(SandEntity), batchSize);

                    if (batchSize == minBatchSize)
                    {
                        var row = batch[0];
                        _logger.LogError(
                            "{Entity} row insert failed: Id={Id}, CustardId={CustardId}",
                            nameof(SandEntity), row.Id, row.CustardId);

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

            // Update existing rows
            int updated = await _context.Database.ExecuteSqlRawAsync($"""
            UPDATE {TableNames.SandEntitiesName}
            SET
                {nameof(SandEntity.CustardId)} = t.{nameof(SandEntity.CustardId)},
                {nameof(SandEntity.Date)} = t.{nameof(SandEntity.Date)},
                {nameof(SandEntity.UnixDate)} = t.{nameof(SandEntity.UnixDate)},
                {nameof(SandEntity.CancelDate)} = t.{nameof(SandEntity.CancelDate)},
                {nameof(SandEntity.UnixCancelDate)} = t.{nameof(SandEntity.UnixCancelDate)},
                {nameof(SandEntity.Active)} = t.{nameof(SandEntity.Active)},
                {nameof(SandEntity.Complete)} = t.{nameof(SandEntity.Complete)},
                {nameof(SandEntity.Value)} = t.{nameof(SandEntity.Value)},
                {nameof(SandEntity.Type)} = t.{nameof(SandEntity.Type)},
                {nameof(SandEntity.Seller)} = t.{nameof(SandEntity.Seller)},
                {nameof(SandEntity.Seller2)} = t.{nameof(SandEntity.Seller2)},
                {nameof(SandEntity.Seller3)} = t.{nameof(SandEntity.Seller3)},
                {nameof(SandEntity.Offerman)} = t.{nameof(SandEntity.Offerman)}
            FROM {tempTable} t
            WHERE t.{nameof(SandEntity.Id)} = {TableNames.SandEntitiesName}.{nameof(SandEntity.Id)};
        """, ct);

            // Insert missing rows
            int inserted = await _context.Database.ExecuteSqlRawAsync($"""
            INSERT INTO {TableNames.SandEntitiesName} (
                {nameof(SandEntity.Id)}, {nameof(SandEntity.CustardId)}, {nameof(SandEntity.Date)}, 
                {nameof(SandEntity.UnixDate)}, {nameof(SandEntity.CancelDate)}, {nameof(SandEntity.UnixCancelDate)},
                {nameof(SandEntity.Active)}, {nameof(SandEntity.Complete)}, {nameof(SandEntity.Value)}, {nameof(SandEntity.Type)}, {nameof(SandEntity.Seller)}, 
                {nameof(SandEntity.Seller2)}, {nameof(SandEntity.Seller3)}, {nameof(SandEntity.Offerman)}
            )
            SELECT
                {nameof(SandEntity.Id)}, {nameof(SandEntity.CustardId)}, {nameof(SandEntity.Date)}, 
                {nameof(SandEntity.UnixDate)}, {nameof(SandEntity.CancelDate)}, {nameof(SandEntity.UnixCancelDate)},
                {nameof(SandEntity.Active)}, {nameof(SandEntity.Complete)}, {nameof(SandEntity.Value)}, {nameof(SandEntity.Type)}, {nameof(SandEntity.Seller)}, 
                {nameof(SandEntity.Seller2)}, {nameof(SandEntity.Seller3)}, {nameof(SandEntity.Offerman)}
            FROM {tempTable} t
            WHERE NOT EXISTS (
                SELECT 1 FROM {TableNames.SandEntitiesName} s WHERE s.{nameof(SandEntity.Id)} = t.{nameof(SandEntity.Id)}
            );
        """, ct);

            // 5️⃣ Clean up temp table
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM {tempTable};", ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Updated={Updated}, Inserted={Inserted}, Skipped={Skipped}",
                nameof(SandEntity), entities.Count, uniqueEntities.Count, stagedCount, updated, inserted, skipped);

            return Result.Success(uniqueEntities);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Entity} upsert failed",nameof(SandEntity));
            return Result.Failure<List<SandEntity>>(ex.ToString());
        }

        void InsertBatch(List<SandEntity> batch)
        {
            var sql = new StringBuilder();
            sql.Append($"INSERT INTO {tempTable} VALUES ");

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];
                sql.Append('(')
                   .Append($"{e.Id}, ")
                   .Append($"{e.CustardId}, ")
                   .Append($"'{e.Date:yyyy-MM-dd HH:mm:ss}', ")
                   .Append($"{e.UnixDate}, ")
                   .Append($"'{e.CancelDate:yyyy-MM-dd HH:mm:ss}', ")
                   .Append($"{e.UnixCancelDate}, ")
                   .Append($"{(e.Active ? 1 : 0)}, ")
                   .Append($"{(e.Complete ? 1 : 0)}, ")
                   .Append($"{e.Value}, ")
                   .Append($"'{Clean(e.Type)}', ")
                   .Append($"{e.Seller}, ")
                   .Append($"{e.Seller2}, ")
                   .Append($"{e.Seller3}, ")
                   .Append($"'{Clean(e.Offerman)}'")
                   .Append(')');

                if (i < batch.Count - 1)
                    sql.Append(", ");
            }

            sql.Append(';');
            _context.Database.ExecuteSqlRaw(sql.ToString());
        }
    }
}
