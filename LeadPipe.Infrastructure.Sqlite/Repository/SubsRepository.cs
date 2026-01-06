using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class SubsRepository(PlumbingContext context, ILogger<SubsRepository> logger)
    : PlumbingContextRepository<SubsEntity, SubsRepository>(context, logger), ISubsRepository
{
    public override async Task<Result<List<SubsEntity>>> UpsertRangeAsync(List<SubsEntity> entities)
    {
        if (entities.Count == 0)
            return Result.Success(new List<SubsEntity>());

        // Deduplicate in-memory by Number
        List<SubsEntity> uniqueEntities = [.. entities
            .GroupBy(e => e.Number)
            .Select(g => g.Last())];

        int batchSize = 50; // Reasonable start given 19 columns per row
        const int minBatchSize = 1;
        int stagedCount = 0;
        int skipped = 0;

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            // Temp table for staging
            await _context.Database.ExecuteSqlRawAsync("""
                CREATE TEMP TABLE IF NOT EXISTS temp_subs_entities (
                    CustomerId INTEGER,
                    Date TEXT,
                    UnixDate INTEGER,
                    SubDate TEXT,
                    UnixSubDate INTEGER,
                    Number INTEGER NOT NULL PRIMARY KEY,
                    Number2 INTEGER,
                    CancelDate TEXT,
                    UnixCancelDate INTEGER,
                    SubCancelDate TEXT,
                    UnixSubCancelDate INTEGER,
                    Active INTEGER,
                    SubActive INTEGER,
                    Complete INTEGER,
                    Value REAL,
                    Type TEXT,
                    Seller INTEGER,
                    Seller2 INTEGER,
                    Seller3 INTEGER
                ) WITHOUT ROWID;
            """);

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

                    if (batchSize < 50)
                        batchSize = Math.Min(batchSize * 2, 50);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Batch insert failed (size={BatchSize}). Reducing batch size.", batchSize);

                    if (batchSize == minBatchSize)
                    {
                        var row = batch[0];
                        _logger.LogError(
                            "Row insert failed: Number={Number}, CustomerId={CustomerId}",
                            row.Number, row.CustomerId);

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

            // ---- Phase 1: UPDATE existing rows ----
            int updated = await _context.Database.ExecuteSqlRawAsync("""
                UPDATE SubsEntities
                SET
                    CustomerId = t.CustomerId,
                    Date = t.Date,
                    UnixDate = t.UnixDate,
                    SubDate = t.SubDate,
                    UnixSubDate = t.UnixSubDate,
                    Number2 = t.Number2,
                    CancelDate = t.CancelDate,
                    UnixCancelDate = t.UnixCancelDate,
                    SubCancelDate = t.SubCancelDate,
                    UnixSubCancelDate = t.UnixSubCancelDate,
                    Active = t.Active,
                    SubActive = t.SubActive,
                    Complete = t.Complete,
                    Value = t.Value,
                    Type = t.Type,
                    Seller = t.Seller,
                    Seller2 = t.Seller2,
                    Seller3 = t.Seller3
                FROM temp_subs_entities t
                WHERE t.Number = SubsEntities.Number;
            """);

            // ---- Phase 2: INSERT missing rows ----
            int inserted = await _context.Database.ExecuteSqlRawAsync("""
                INSERT INTO SubsEntities (
                    CustomerId, Date, UnixDate, SubDate, UnixSubDate, Number, Number2, CancelDate, UnixCancelDate,
                    SubCancelDate, UnixSubCancelDate, Active, SubActive, Complete, Value, Type, Seller, Seller2, Seller3
                )
                SELECT *
                FROM temp_subs_entities t
                WHERE NOT EXISTS (
                    SELECT 1 FROM SubsEntities s WHERE s.Number = t.Number
                );
            """);

            await _context.Database.ExecuteSqlRawAsync("DELETE FROM temp_subs_entities;");
            await transaction.CommitAsync();

            _logger.LogInformation(
                "SubsEntity upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Updated={Updated}, Inserted={Inserted}, Skipped={Skipped}",
                entities.Count, uniqueEntities.Count, stagedCount, updated, inserted, skipped);

            return Result.Success(uniqueEntities);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SubsEntity upsert failed");
            return Result.Failure<List<SubsEntity>>(ex.ToString());
        }

        void InsertBatch(List<SubsEntity> batch)
        {
            var sql = new StringBuilder();
            sql.Append("INSERT INTO temp_subs_entities VALUES ");

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];
                sql.Append("(")
                   .Append($"{e.CustomerId}, ")
                   .Append($"'{e.Date:yyyy-MM-dd HH:mm:ss}', ")
                   .Append($"{e.UnixDate}, ")
                   .Append($"'{e.SubDate:yyyy-MM-dd HH:mm:ss}', ")
                   .Append($"{e.UnixSubDate}, ")
                   .Append($"{e.Number}, ")
                   .Append($"{e.Number2}, ")
                   .Append($"'{e.CancelDate:yyyy-MM-dd HH:mm:ss}', ")
                   .Append($"{e.UnixCancelDate}, ")
                   .Append($"'{e.SubCancelDate:yyyy-MM-dd HH:mm:ss}', ")
                   .Append($"{e.UnixSubCancelDate}, ")
                   .Append($"{(e.Active ? 1 : 0)}, ")
                   .Append($"{(e.SubActive ? 1 : 0)}, ")
                   .Append($"{(e.Complete ? 1 : 0)}, ")
                   .Append($"{e.Value}, ")
                   .Append($"'{e.Type?.Replace("'", "''") ?? ""}', ")
                   .Append($"{e.Seller}, ")
                   .Append($"{e.Seller2}, ")
                   .Append($"{e.Seller3}")
                   .Append(")");

                if (i < batch.Count - 1)
                    sql.Append(", ");
            }

            sql.Append(';');
            _context.Database.ExecuteSqlRaw(sql.ToString());
        }
    }
}
