using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public class SubsPlumbingLinkRepository(PlumbingContext context, ILogger<SubsPlumbingLinkRepository> logger)
    : PlumbingContextRepository<SubsPlumbingLink, SubsPlumbingLinkRepository>(context, logger), ISubsPlumbingLinkRepository
{
    public async Task<Result<List<SubsPlumbingLink>>> GetAllWithDetailsAsync()
    {
        try
        {
            List<SubsPlumbingLink> list = await _context.SubsPlumbingLinks
                .AsNoTracking()
                .Include(p => p.PlumbingEntity)
                .Include(p => p.SubsEntity)
                .ToListAsync();
            return list;
        }
        catch (Exception ex) { return Result.Failure<List<SubsPlumbingLink>>(ex.ToString()); }
    }
    public override async Task<Result<List<SubsPlumbingLink>>> GetAllAsync()
    {
        try
        {
            List<SubsPlumbingLink> list = await _context.SubsPlumbingLinks
                .AsNoTracking()
                .Select(s => new SubsPlumbingLink() { Id = s.Id, SubsId = s.SubsId, PlumbingId = s.PlumbingId, MatchingSubPhone = s.MatchingSubPhone })
                .ToListAsync();
            return list;
        }
        catch (Exception ex) { return Result.Failure<List<SubsPlumbingLink>>(ex.Message); }
    }
    public override async Task<Result<List<SubsPlumbingLink>>> UpsertRangeAsync(List<SubsPlumbingLink> entities)
    {
        if (entities.Count == 0)
            return Result.Success(new List<SubsPlumbingLink>());

        // Deduplicate in-memory by (SubsId, PlumbingId)
        List<SubsPlumbingLink> uniqueEntities = [.. entities
            .GroupBy(e => (e.SubsId, e.PlumbingId))
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
                CREATE TEMP TABLE IF NOT EXISTS temp_subs_plumbing_links (
                    SubsId INTEGER NOT NULL,
                    PlumbingId INTEGER NOT NULL,
                    MatchingSubPhone INTEGER NOT NULL,
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
                    _logger.LogWarning(ex, "Batch insert failed (size={BatchSize}). Reducing batch size.", batchSize);

                    if (batchSize == minBatchSize)
                    {
                        var row = batch[0];
                        _logger.LogError(
                            "Row insert failed: SubsId={SubsId}, PlumbingId={PlumbingId}, MatchingSubPhone={MatchingSubPhone}",
                            row.SubsId, row.PlumbingId, row.MatchingSubPhone);

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
                SET MatchingSubPhone = (
                    SELECT t.MatchingSubPhone
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
                INSERT INTO SubsPlumbingLinks (SubsId, PlumbingId, MatchingSubPhone)
                SELECT t.SubsId, t.PlumbingId, t.MatchingSubPhone
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
                "SubsPlumbingLink upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Updated={Updated}, Inserted={Inserted}, Skipped={Skipped}",
                entities.Count, uniqueEntities.Count, stagedCount, updated, inserted, skipped);

            return Result.Success(uniqueEntities);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SubsPlumbingLink upsert failed");
            return Result.Failure<List<SubsPlumbingLink>>(ex.Message);
        }

        void InsertBatch(List<SubsPlumbingLink> batch)
        {
            var sql = new StringBuilder();
            sql.Append("INSERT INTO temp_subs_plumbing_links VALUES ");

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];
                sql.Append($"({e.SubsId}, {e.PlumbingId}, {e.MatchingSubPhone})");

                if (i < batch.Count - 1)
                    sql.Append(", ");
            }

            sql.Append(';');
            _context.Database.ExecuteSqlRaw(sql.ToString());
        }
    }
    public async Task<Result<List<SubsPlumbingLink>>> GetAllAsync(IEnumerable<PlumbingEntity> filter)
    {
        try
        {
            HashSet<long> ids = [.. filter.Select(p => p.Id)];
            List<SubsPlumbingLink> set = await _set
                .Where(e => ids.Contains(e.PlumbingId))
                .ToListAsync();
            return Result.Success(set);
        }
        catch (Exception ex) { return Result.Failure<List<SubsPlumbingLink>>(ex.Message); }
    }
}
