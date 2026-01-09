using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public class SandPlumbingLinkRepository(PlumbingContext context, ILogger<SandPlumbingLinkRepository> logger)
    : PlumbingContextRepository<SandPlumbingLink, SandPlumbingLinkRepository>(context, logger), ISandPlumbingLinkRepository
{
    public async Task<Result<List<SandPlumbingLink>>> GetAllWithDetailsAsync()
    {
        try
        {
            List<SandPlumbingLink> list = await _context.SandPlumbingLinks
                .AsNoTracking()
                .Include(p => p.PlumbingEntity)
                .Include(p => p.SandEntity)
                .ToListAsync();
            return list;
        }
        catch (Exception ex) { return Result.Failure<List<SandPlumbingLink>>(ex.ToString()); }
    }
    public async Task<Result<List<SandPlumbingLink>>> GetAllWithDetailsAsync(IEnumerable<PlumbingEntity> filter)
    {
        try
        {
            List<long> ids = [.. filter.Select(p => p.Id)];
            List<SandPlumbingLink> list = await _context.SandPlumbingLinks
                .AsNoTracking()
                .Where(e => ids.Contains(e.PlumbingId))
                .Include(p => p.PlumbingEntity)
                .Include(p => p.SandEntity)
                .ToListAsync();
            return list;
        }
        catch (Exception ex) { return Result.Failure<List<SandPlumbingLink>>(ex.ToString()); }
    }
    public override async Task<Result<List<SandPlumbingLink>>> GetAllAsync()
    {
        try
        {
            List<SandPlumbingLink> list = await _context.SandPlumbingLinks
                .AsNoTracking()
                .Select(s => new SandPlumbingLink()
                {
                    Id = s.Id,
                    SandId = s.SandId,
                    PlumbingId = s.PlumbingId,
                    MatchingPhone = s.MatchingPhone
                })
                .ToListAsync();
            return list;
        }
        catch (Exception ex) { return Result.Failure<List<SandPlumbingLink>>(ex.Message); }
    }
    public override async Task<Result<List<SandPlumbingLink>>> UpsertRangeAsync(List<SandPlumbingLink> entities)
    {
        if (entities.Count == 0)
            return Result.Success(new List<SandPlumbingLink>());

        // Deduplicate in-memory by (SubsId, PlumbingId)
        List<SandPlumbingLink> uniqueEntities =
        [
            .. entities
                .GroupBy(e => (e.SandId, e.PlumbingId))
                .Select(g => g.Last())
        ];

        int batchSize = 200;
        const int minBatchSize = 1;
        int stagedCount = 0;
        int skipped = 0;

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            // Temp table for staging
            await _context.Database.ExecuteSqlRawAsync("""
                CREATE TEMP TABLE IF NOT EXISTS temp_subs_plumbing_links (
                    SubsId INTEGER NOT NULL,
                    PlumbingId INTEGER NOT NULL,
                    MatchingPhone INTEGER NOT NULL,
                    PRIMARY KEY (SubsId, PlumbingId)
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
                    _logger.LogWarning(
                        ex,
                        "{Entity} batch insert failed (size={BatchSize}). Reducing batch size.",
                       nameof(SandPlumbingLink),
                       batchSize);

                    if (batchSize == minBatchSize)
                    {
                        var row = batch[0];
                        _logger.LogError(
                            "{Entity} row insert failed: SubsId={SubsId}, PlumbingId={PlumbingId}, MatchingPhone={MatchingPhone}",
                            nameof(SandPlumbingLink),
                            row.SandId,
                            row.PlumbingId,
                            row.MatchingPhone);

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
                UPDATE SubsPlumbingLinks
                SET MatchingPhone = (
                    SELECT t.MatchingPhone
                    FROM temp_subs_plumbing_links t
                    WHERE t.SubsId = SubsPlumbingLinks.SubsId
                      AND t.PlumbingId = SubsPlumbingLinks.PlumbingId
                )
                WHERE EXISTS (
                    SELECT 1
                    FROM temp_subs_plumbing_links t
                    WHERE t.SubsId = SubsPlumbingLinks.SubsId
                      AND t.PlumbingId = SubsPlumbingLinks.PlumbingId
                );
            """);

            // ---- Phase 2: INSERT missing rows ----
            int inserted = await _context.Database.ExecuteSqlRawAsync("""
                INSERT INTO SubsPlumbingLinks (SubsId, PlumbingId, MatchingPhone)
                SELECT t.SubsId, t.PlumbingId, t.MatchingPhone
                FROM temp_subs_plumbing_links t
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM SubsPlumbingLinks s
                    WHERE s.SubsId = t.SubsId
                      AND s.PlumbingId = t.PlumbingId
                );
            """);

            await _context.Database.ExecuteSqlRawAsync("DELETE FROM temp_subs_plumbing_links;");
            await transaction.CommitAsync();

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Updated={Updated}, Inserted={Inserted}, Skipped={Skipped}",
                nameof(SandPlumbingLink),
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
            _logger.LogError(ex, "{Entity} upsert failed",
                nameof(SandPlumbingLink));
            return Result.Failure<List<SandPlumbingLink>>(ex.Message);
        }

        void InsertBatch(List<SandPlumbingLink> batch)
        {
            var sql = new StringBuilder();
            sql.Append("INSERT INTO temp_subs_plumbing_links VALUES ");

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];
                sql.Append($"({e.SandId}, {e.PlumbingId}, {e.MatchingPhone})");

                if (i < batch.Count - 1)
                    sql.Append(", ");
            }

            sql.Append(';');
            _context.Database.ExecuteSqlRaw(sql.ToString());
        }
    }
    public async Task<Result<List<SandPlumbingLink>>> GetAllAsync(IEnumerable<PlumbingEntity> filter)
    {
        try
        {
            List<long> ids = [.. filter.Select(p => p.Id)];
            List<SandPlumbingLink> set = await _set
                .Where(e => ids.Contains(e.PlumbingId))
                .ToListAsync();
            return Result.Success(set);
        }
        catch (Exception ex) { return Result.Failure<List<SandPlumbingLink>>(ex.Message); }
    }
}
