using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class PlumbingCallLinkRepository(
    PlumbingContext context,
    ILogger<PlumbingCallLinkRepository> logger)
    : PlumbingContextRepository<PlumbingCallLink, PlumbingCallLinkRepository>(context, logger),
      IPlumbingCallLinkRepository
{
    public override async Task<Result<List<PlumbingCallLink>>> GetAllAsync()
    {
        try
        {
            List<PlumbingCallLink> list = await _context.PlumbingCallLinks
                .AsNoTracking()
                .Select(s => new PlumbingCallLink() { Id = s.Id, CallId = s.CallId, PlumbingId = s.PlumbingId })
                .ToListAsync();
            return list;
        }
        catch (Exception ex) { return Result.Failure<List<PlumbingCallLink>>(ex.Message); }
    }
    public override async Task<Result<List<PlumbingCallLink>>> UpsertRangeAsync(
        List<PlumbingCallLink> entities)
    {
        if (entities.Count == 0)
            return Result.Success(new List<PlumbingCallLink>());

        // Deduplicate in-memory
        List<PlumbingCallLink> uniqueEntities =
        [
            .. entities
                .GroupBy(e => (e.PlumbingId, e.CallId))
                .Select(g => g.Last())
        ];

        int batchSize = 200;
        const int minBatchSize = 1;

        try
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            // Temp table (connection-scoped)
            await _context.Database.ExecuteSqlRawAsync("""
            CREATE TEMP TABLE IF NOT EXISTS temp_plumbing_call_links (
                PlumbingId INTEGER NOT NULL,
                CallId INTEGER NOT NULL,
                PRIMARY KEY (PlumbingId, CallId)
            ) WITHOUT ROWID;
            """);

            int index = 0;
            int stagedCount = 0;
            int skipped = 0;

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
                        "Batch insert failed (size={BatchSize}). Reducing batch size.",
                        batchSize);

                    if (batchSize == minBatchSize)
                    {
                        var row = batch[0];

                        _logger.LogError(
                            "Row insert failed: PlumbingId={PlumbingId}, CallId={CallId}",
                            row.PlumbingId,
                            row.CallId);

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

            // ---- Phase 1: UPDATE (no-op but keeps symmetry & metrics) ----
            int updated = await _context.Database.ExecuteSqlRawAsync("""
            UPDATE PlumbingCallLinks
            SET
                PlumbingId = (
                    SELECT t.PlumbingId
                    FROM temp_plumbing_call_links t
                    WHERE t.PlumbingId = PlumbingCallLinks.PlumbingId
                      AND t.CallId = PlumbingCallLinks.CallId
                ),
                CallId = (
                    SELECT t.CallId
                    FROM temp_plumbing_call_links t
                    WHERE t.PlumbingId = PlumbingCallLinks.PlumbingId
                      AND t.CallId = PlumbingCallLinks.CallId
                )
            WHERE EXISTS (
                SELECT 1
                FROM temp_plumbing_call_links t
                WHERE t.PlumbingId = PlumbingCallLinks.PlumbingId
                  AND t.CallId = PlumbingCallLinks.CallId
            );
            """);

            // ---- Phase 2: INSERT missing rows ----
            int inserted = await _context.Database.ExecuteSqlRawAsync("""
            INSERT INTO PlumbingCallLinks (PlumbingId, CallId)
            SELECT
                t.PlumbingId,
                t.CallId
            FROM temp_plumbing_call_links t
            WHERE NOT EXISTS (
                SELECT 1
                FROM PlumbingCallLinks p
                WHERE p.PlumbingId = t.PlumbingId
                  AND p.CallId = t.CallId
            );
            """);

            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM temp_plumbing_call_links;");

            await transaction.CommitAsync();

            _logger.LogInformation(
                "PlumbingCallLink upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Updated={Updated}, Inserted={Inserted}, Skipped={Skipped}",
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
            _logger.LogError(ex, "PlumbingCallLink upsert failed");
            return Result.Failure<List<PlumbingCallLink>>(ex.ToString());
        }

        // ---- Local helper ----
        void InsertBatch(List<PlumbingCallLink> batch)
        {
            var sql = new StringBuilder();
            sql.Append("INSERT INTO temp_plumbing_call_links VALUES ");

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];

                sql.Append($"({e.PlumbingId}, {e.CallId})");

                if (i < batch.Count - 1)
                    sql.Append(", ");
            }

            sql.Append(';');
            _context.Database.ExecuteSqlRaw(sql.ToString());
        }
    }
}
