using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class SandRepository(PlumbingContext context, ILogger<SandRepository> logger)
    : PlumbingContextRepository<SandEntity, SandRepository>(context, logger), IRepository<SandEntity>
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
            return Result.Success<List<SandEntity>>([]);

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

        List<SandEntity> uniqueEntities = 
        [
            .. entities
                .GroupBy(e => e.Id)
                .Select(g => g.Last())
        ];

        HashSet<long> neededCustardIds = [.. uniqueEntities.Select(e => e.CustardId)];
        HashSet<long> existingCustardIds = [.. _context.CustardEntities
            .Where(c => neededCustardIds.Contains(c.Id)).Select(c => c.Id)];

        List<SandEntity> validEntities = new(uniqueEntities.Count);
        List<SandEntity> rejectedEntities = new(uniqueEntities.Count);

        foreach (var e in uniqueEntities)
        {
            if (existingCustardIds.Contains(e.CustardId))
                validEntities.Add(e);
            else
                rejectedEntities.Add(e);
        }

        int rejected = rejectedEntities.Count;

        if (rejected > 0)
        {
            _logger.LogWarning("{Entity}: {Rejected} rows skipped due to invalid Id: {CustardId}. Skipped rows (as Id, CustardId) = {Rows}",
                nameof(SandEntity), 
                rejected,
                nameof(SandEntity.CustardId),
                rejectedEntities
                    .Select(e=> new {e.Id, e.CustardId})
                    .ToArray());

        }
        uniqueEntities = validEntities;

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
                    {nameof(SandEntity.Value)} TEXT,
                    {nameof(SandEntity.Type)} TEXT,
                    {nameof(SandEntity.Seller)} INTEGER,
                    {nameof(SandEntity.Seller2)} INTEGER,
                    {nameof(SandEntity.Seller3)} INTEGER,
                    {nameof(SandEntity.Offerman)} TEXT NOT NULL
                ) WITHOUT ROWID;
                DELETE FROM {tempTable};
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
                    _logger.LogError(ex, "{Entity} batch insert failed (size={BatchSize}). Reducing batch size.", 
                        nameof(SandEntity), 
                        batchSize);

                    if (batchSize == minBatchSize)
                    {
                        var row = batch[0];
                        _logger.LogError(
                            "{Entity} row insert failed: Id={Id}, CustardId={CustardId}",
                            nameof(SandEntity),
                            row.Id,
                            row.CustardId);

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

            int totalAffected = await _context.Database.ExecuteSqlRawAsync($"""
                INSERT INTO {TableNames.SandEntitiesName} (
                    {nameof(SandEntity.Id)}, 
                    {nameof(SandEntity.CustardId)}, 
                    {nameof(SandEntity.Date)}, 
                    {nameof(SandEntity.UnixDate)}, 
                    {nameof(SandEntity.CancelDate)}, 
                    {nameof(SandEntity.UnixCancelDate)}, 
                    {nameof(SandEntity.Active)}, 
                    {nameof(SandEntity.Complete)}, 
                    {nameof(SandEntity.Value)}, 
                    {nameof(SandEntity.Type)}, 
                    {nameof(SandEntity.Seller)}, 
                    {nameof(SandEntity.Seller2)}, 
                    {nameof(SandEntity.Seller3)}, 
                    {nameof(SandEntity.Offerman)}
                )
                SELECT 
                    {nameof(SandEntity.Id)}, 
                    {nameof(SandEntity.CustardId)}, 
                    {nameof(SandEntity.Date)}, 
                    {nameof(SandEntity.UnixDate)}, 
                    {nameof(SandEntity.CancelDate)}, 
                    {nameof(SandEntity.UnixCancelDate)}, 
                    {nameof(SandEntity.Active)}, 
                    {nameof(SandEntity.Complete)}, 
                    {nameof(SandEntity.Value)}, 
                    {nameof(SandEntity.Type)}, 
                    {nameof(SandEntity.Seller)}, 
                    {nameof(SandEntity.Seller2)}, 
                    {nameof(SandEntity.Seller3)}, 
                    {nameof(SandEntity.Offerman)}
                FROM {tempTable}
                ON CONFLICT(Id) DO UPDATE SET
                    {nameof(SandEntity.CustardId)} = excluded.{nameof(SandEntity.CustardId)},
                    {nameof(SandEntity.Date)} = excluded.{nameof(SandEntity.Date)},
                    {nameof(SandEntity.UnixDate)} = excluded.{nameof(SandEntity.UnixDate)},
                    {nameof(SandEntity.CancelDate)} = excluded.{nameof(SandEntity.CancelDate)},
                    {nameof(SandEntity.UnixCancelDate)} = excluded.{nameof(SandEntity.UnixCancelDate)},
                    {nameof(SandEntity.Active)} = excluded.{nameof(SandEntity.Active)},
                    {nameof(SandEntity.Complete)} = excluded.{nameof(SandEntity.Complete)},
                    {nameof(SandEntity.Value)} = excluded.{nameof(SandEntity.Value)},
                    {nameof(SandEntity.Type)} = excluded.{nameof(SandEntity.Type)},
                    {nameof(SandEntity.Seller)} = excluded.{nameof(SandEntity.Seller)},
                    {nameof(SandEntity.Seller2)} = excluded.{nameof(SandEntity.Seller2)},
                    {nameof(SandEntity.Seller3)} = excluded.{nameof(SandEntity.Seller3)},
                    {nameof(SandEntity.Offerman)} = excluded.{nameof(SandEntity.Offerman)};
            """, ct);

            // 5️⃣ Clean up temp table
            await _context.Database.ExecuteSqlRawAsync($"DELETE FROM {tempTable};", ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Affected={Affected}, Skipped={Skipped}",
                nameof(SandEntity), 
                entities.Count, 
                uniqueEntities.Count, 
                stagedCount, 
                totalAffected, 
                skipped);

            return Result.Success(uniqueEntities);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Entity} upsert failed. Exception Message: {Message}", nameof(SandEntity), ex.Message);
            return Result.Failure<List<SandEntity>>(ex.ToString());
        }

        void InsertBatch(List<SandEntity> batch)
        {
            var values = new List<object>();
            var rows = new List<string>();
            const int cols = 14;

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];
                int o = i * cols;
                rows.Add($"({{{o}}},{{{o + 1}}},{{{o + 2}}},{{{o + 3}}},{{{o + 4}}},{{{o + 5}}},{{{o + 6}}},{{{o + 7}}},{{{o + 8}}},{{{o + 9}}},{{{o + 10}}},{{{o + 11}}},{{{o + 12}}},{{{o + 13}}})");

                values.Add(e.Id);
                values.Add(e.CustardId);
                values.Add(e.Date.ToString("yyyy-MM-dd HH:mm:ss"));
                values.Add(e.UnixDate);
                values.Add(e.CancelDate == default ? DBNull.Value : e.CancelDate.ToString("yyyy-MM-dd HH:mm:ss"));
                values.Add(e.UnixCancelDate);
                values.Add(e.Active ? 1 : 0);
                values.Add(e.Complete ? 1 : 0);
                values.Add(e.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                values.Add(e.Type ?? (object)DBNull.Value);
                values.Add(e.Seller);
                values.Add(e.Seller2);
                values.Add(e.Seller3);
                values.Add(e.Offerman ?? string.Empty);
            }

            string joined = $"INSERT INTO {tempTable} VALUES {string.Join(",", rows)};";
            _context.Database.ExecuteSqlRaw(joined, [.. values]);
        }
    }

}
