using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class SubsCallLinkRepository(PlumbingContext context, ILogger<SubsCallLinkRepository> logger)
    : PlumbingContextRepository<CallSubsLink, SubsCallLinkRepository>(context, logger), ISubsCallLinkRepository
{
    public async Task<Result<List<CallSubsLink>>> GetAllWithDetailsAsync()
    {
        try
        {
            List<CallSubsLink> list = await _context.SubsCallLinks
                .AsNoTracking()
                .Include(p => p.CallEntity)
                .Include(p => p.SubsEntity)
                .ToListAsync();
            return list;
        }
        catch (Exception ex) { return Result.Failure<List<CallSubsLink>>(ex.ToString()); }
    }
    public async Task<Result<List<CallSubsLink>>> GetAllWithDetailsAsync(List<CallEntity> list)
    {
        try
        {
            List<long> ids = [.. list.Select(l => l.Id)];
            List<CallSubsLink> result = await _context.SubsCallLinks
                .AsNoTracking()
                .Where(p => ids.Contains(p.CallId))
                .Include(p => p.CallEntity)
                .Include(p => p.SubsEntity)
                .ToListAsync();
            return result;
        }
        catch (Exception ex) { return Result.Failure<List<CallSubsLink>>(ex.ToString()); }
    }
    public override async Task<Result<List<CallSubsLink>>> GetAllAsync()
    {
        try
        {
            List<CallSubsLink> list = await _context.SubsCallLinks
                .AsNoTracking()
                .Select(s => new CallSubsLink() { Id = s.Id, SubsId = s.SubsId, CallId = s.CallId, MatchingNumber = s.MatchingNumber })
                .ToListAsync();
            return list;
        }
        catch (Exception ex) { return Result.Failure<List<CallSubsLink>>(ex.Message); }
    }
    public override async Task<Result<List<CallSubsLink>>> UpsertRangeAsync(List<CallSubsLink> entities)
    {
        if (entities.Count == 0)
            return Result.Success(new List<CallSubsLink>());

        // Deduplicate in-memory by (SubsId, CallId)
        List<CallSubsLink> uniqueEntities = [.. entities
            .GroupBy(e => (e.SubsId, e.CallId))
            .Select(g => g.Last())];

        int batchSize = 200;
        const int minBatchSize = 1;
        int stagedCount = 0;
        int skipped = 0;

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            // Temp table for staging
            await _context.Database.ExecuteSqlRawAsync("""
                CREATE TEMP TABLE IF NOT EXISTS temp_subs_call_links (
                    SubsId INTEGER NOT NULL,
                    CallId INTEGER NOT NULL,
                    MatchingNumber INTEGER NOT NULL,
                    PRIMARY KEY (SubsId, CallId)
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

                    if (batchSize < 200)
                        batchSize = Math.Min(batchSize * 2, 200);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Batch insert failed (size={BatchSize}). Reducing batch size.", batchSize);

                    if (batchSize == minBatchSize)
                    {
                        var row = batch[0];
                        _logger.LogError(
                            "Row insert failed: SubsId={SubsId}, CallId={CallId}, MatchingNumber={MatchingNumber}",
                            row.SubsId, row.CallId, row.MatchingNumber);

                        index++;
                        batchSize = 100;
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
                UPDATE SubsCallLinks
                SET MatchingNumber = (
                    SELECT t.MatchingNumber
                    FROM temp_subs_call_links t
                    WHERE t.SubsId = SubsCallLinks.SubsId
                      AND t.CallId = SubsCallLinks.CallId
                )
                WHERE EXISTS (
                    SELECT 1
                    FROM temp_subs_call_links t
                    WHERE t.SubsId = SubsCallLinks.SubsId
                      AND t.CallId = SubsCallLinks.CallId
                );
            """);

            // ---- Phase 2: INSERT missing rows ----
            int inserted = await _context.Database.ExecuteSqlRawAsync("""
                INSERT INTO SubsCallLinks (SubsId, CallId, MatchingNumber)
                SELECT t.SubsId, t.CallId, t.MatchingNumber
                FROM temp_subs_call_links t
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM SubsCallLinks c
                    WHERE c.SubsId = t.SubsId
                      AND c.CallId = t.CallId
                );
            """);

            await _context.Database.ExecuteSqlRawAsync("DELETE FROM temp_subs_call_links;");
            await transaction.CommitAsync();

            _logger.LogInformation(
                "SubsCallLink upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Updated={Updated}, Inserted={Inserted}, Skipped={Skipped}",
                entities.Count, uniqueEntities.Count, stagedCount, updated, inserted, skipped);

            return Result.Success(uniqueEntities);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CallSubsLink upsert failed");
            return Result.Failure<List<CallSubsLink>>(ex.Message);
        }

        void InsertBatch(List<CallSubsLink> batch)
        {
            var sql = new StringBuilder();
            sql.Append("INSERT INTO temp_subs_call_links VALUES ");

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];
                sql.Append($"({e.SubsId}, {e.CallId}, {e.MatchingNumber})");

                if (i < batch.Count - 1)
                    sql.Append(", ");
            }

            sql.Append(';');
            _context.Database.ExecuteSqlRaw(sql.ToString());
        }
    }
}
