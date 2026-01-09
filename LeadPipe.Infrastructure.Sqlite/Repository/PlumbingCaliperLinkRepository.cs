using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class PlumbingCaliperLinkRepository(
    PlumbingContext context,
    ILogger<PlumbingCaliperLinkRepository> logger)
    : PlumbingContextRepository<PlumbingCaliperLink, PlumbingCaliperLinkRepository>(context, logger),
      IPlumbingCaliperLinkRepository
{
    public async Task<Result<List<PlumbingCaliperLink>>> GetAllWithDetailsAsync()
    {
        try
        {
            List<PlumbingCaliperLink> list = await _context.PlumbingCaliperLinks
                .AsNoTracking()
                .Include(p => p.CaliperEntity)
                .Include(p => p.PlumbingEntity)
                .ToListAsync();
            return list;
        }
        catch (Exception ex) { return Result.Failure<List<PlumbingCaliperLink>>(ex.ToString()); }
    }
    public override async Task<Result<List<PlumbingCaliperLink>>> GetAllAsync()
    {
        try
        {
            List<PlumbingCaliperLink> list = await _context.PlumbingCaliperLinks
                .AsNoTracking()
                .Select(s => new PlumbingCaliperLink() { Id = s.Id, CaliperId = s.CaliperId, PlumbingId = s.PlumbingId })
                .ToListAsync();
            return list;
        }
        catch (Exception ex) { return Result.Failure<List<PlumbingCaliperLink>>(ex.ToString()); }
    }
    public override async Task<Result<List<PlumbingCaliperLink>>> UpsertRangeAsync(
        List<PlumbingCaliperLink> entities)
    {
        if (entities.Count == 0)
            return Result.Success(new List<PlumbingCaliperLink>());

        // Deduplicate in-memory
        List<PlumbingCaliperLink> uniqueEntities =
        [
            .. entities
                .GroupBy(e => (e.PlumbingId, e.CaliperId))
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
                CaliperId INTEGER NOT NULL,
                PRIMARY KEY (PlumbingId, CaliperId)
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
                            "Row insert failed: PlumbingId={PlumbingId}, CaliperId={CaliperId}",
                            row.PlumbingId,
                            row.CaliperId);

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
            UPDATE PlumbingCaliperLinks
            SET
                PlumbingId = (
                    SELECT t.PlumbingId
                    FROM temp_plumbing_call_links t
                    WHERE t.PlumbingId = PlumbingCaliperLinks.PlumbingId
                      AND t.CaliperId = PlumbingCaliperLinks.CaliperId
                ),
                CaliperId = (
                    SELECT t.CaliperId
                    FROM temp_plumbing_call_links t
                    WHERE t.PlumbingId = PlumbingCaliperLinks.PlumbingId
                      AND t.CaliperId = PlumbingCaliperLinks.CaliperId
                )
            WHERE EXISTS (
                SELECT 1
                FROM temp_plumbing_call_links t
                WHERE t.PlumbingId = PlumbingCaliperLinks.PlumbingId
                  AND t.CaliperId = PlumbingCaliperLinks.CaliperId
            );
            """);

            // ---- Phase 2: INSERT missing rows ----
            int inserted = await _context.Database.ExecuteSqlRawAsync("""
            INSERT INTO PlumbingCaliperLinks (PlumbingId, CaliperId)
            SELECT
                t.PlumbingId,
                t.CaliperId
            FROM temp_plumbing_call_links t
            WHERE NOT EXISTS (
                SELECT 1
                FROM PlumbingCaliperLinks p
                WHERE p.PlumbingId = t.PlumbingId
                  AND p.CaliperId = t.CaliperId
            );
            """);

            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM temp_plumbing_call_links;");

            await transaction.CommitAsync();

            _logger.LogInformation(
                "PlumbingCaliperLink upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Updated={Updated}, Inserted={Inserted}, Skipped={Skipped}",
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
            _logger.LogError(ex, "PlumbingCaliperLink upsert failed");
            return Result.Failure<List<PlumbingCaliperLink>>(ex.ToString());
        }

        // ---- Local helper ----
        void InsertBatch(List<PlumbingCaliperLink> batch)
        {
            var sql = new StringBuilder();
            sql.Append("INSERT INTO temp_plumbing_call_links VALUES ");

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];

                sql.Append($"({e.PlumbingId}, {e.CaliperId})");

                if (i < batch.Count - 1)
                    sql.Append(", ");
            }

            sql.Append(';');
            _context.Database.ExecuteSqlRaw(sql.ToString());
        }
    }
}
