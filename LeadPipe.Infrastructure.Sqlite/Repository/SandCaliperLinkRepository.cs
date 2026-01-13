using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class SubsCaliperLinkRepository(PlumbingContext context, ILogger<SubsCaliperLinkRepository> logger)
    : PlumbingContextRepository<SandCaliperLink, SubsCaliperLinkRepository>(context, logger), ISandCaliperLinkRepository
{
    public async Task<Result<List<SandCaliperLink>>> GetAllWithDetailsAsync()
    {
        try
        {
            List<SandCaliperLink> list = await _context.SandCaliperLinks
                .AsNoTracking()
                .Include(p => p.CaliperEntity)
                .Include(p => p.SandEntity)
                .ToListAsync();
            return list;
        }
        catch (Exception ex) { return Result.Failure<List<SandCaliperLink>>(ex.ToString()); }
    }
    public async Task<Result<List<SandCaliperLink>>> GetAllWithDetailsAsync(List<CaliperEntity> list)
    {
        try
        {
            List<long> ids = [.. list.Select(l => l.Id)];
            List<SandCaliperLink> result = await _context.SandCaliperLinks
                .AsNoTracking()
                .Where(p => ids.Contains(p.CaliperId))
                .Include(p => p.CaliperEntity)
                .Include(p => p.SandEntity)
                .ToListAsync();
            return result;
        }
        catch (Exception ex) { return Result.Failure<List<SandCaliperLink>>(ex.ToString()); }
    }
    public override async Task<Result<List<SandCaliperLink>>> GetAllAsync()
    {
        try
        {
            List<SandCaliperLink> list = await _context.SandCaliperLinks
                .AsNoTracking()
                .Select(s => new SandCaliperLink() { Id = s.Id, SandId = s.SandId, CaliperId = s.CaliperId, MatchingPhone = s.MatchingPhone })
                .ToListAsync();
            return list;
        }
        catch (Exception ex) { return Result.Failure<List<SandCaliperLink>>(ex.Message); }
    }
    public override async Task<Result<List<SandCaliperLink>>> UpsertRangeAsync(List<SandCaliperLink> entities)
    {
        if (entities.Count == 0)
            return Result.Success(new List<SandCaliperLink>());

        // Deduplicate in-memory by (SubsId, CaliperId)
        List<SandCaliperLink> uniqueEntities = [.. entities
            .GroupBy(e => (e.SandId, e.CaliperId))
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
                    CaliperId INTEGER NOT NULL,
                    MatchingNumber INTEGER NOT NULL,
                    PRIMARY KEY (SubsId, CaliperId)
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
                            "Row insert failed: SubsId={SubsId}, CaliperId={CaliperId}, MatchingNumber={MatchingNumber}",
                            row.SandId, row.CaliperId, row.MatchingPhone);

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
                UPDATE SubsCaliperLinks
                SET MatchingNumber = (
                    SELECT t.MatchingNumber
                    FROM temp_subs_call_links t
                    WHERE t.SubsId = SubsCaliperLinks.SubsId
                      AND t.CaliperId = SubsCaliperLinks.CaliperId
                )
                WHERE EXISTS (
                    SELECT 1
                    FROM temp_subs_call_links t
                    WHERE t.SubsId = SubsCaliperLinks.SubsId
                      AND t.CaliperId = SubsCaliperLinks.CaliperId
                );
            """);

            // ---- Phase 2: INSERT missing rows ----
            int inserted = await _context.Database.ExecuteSqlRawAsync("""
                INSERT INTO SubsCaliperLinks (SubsId, CaliperId, MatchingNumber)
                SELECT t.SubsId, t.CaliperId, t.MatchingNumber
                FROM temp_subs_call_links t
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM SubsCaliperLinks c
                    WHERE c.SubsId = t.SubsId
                      AND c.CaliperId = t.CaliperId
                );
            """);

            await _context.Database.ExecuteSqlRawAsync("DELETE FROM temp_subs_call_links;");
            await transaction.CommitAsync();

            _logger.LogInformation(
                "SubsCaliperLink upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Updated={Updated}, Inserted={Inserted}, Skipped={Skipped}",
                entities.Count, uniqueEntities.Count, stagedCount, updated, inserted, skipped);

            return Result.Success(uniqueEntities);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CaliperSubsLink upsert failed");
            return Result.Failure<List<SandCaliperLink>>(ex.Message);
        }

        void InsertBatch(List<SandCaliperLink> batch)
        {
            var sql = new StringBuilder();
            sql.Append("INSERT INTO temp_subs_call_links VALUES ");

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];
                sql.Append($"({e.SandId}, {e.CaliperId}, {e.MatchingPhone})");

                if (i < batch.Count - 1)
                    sql.Append(", ");
            }

            sql.Append(';');
            _context.Database.ExecuteSqlRaw(sql.ToString());
        }
    }
}
