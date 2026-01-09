using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public class SandCornLinkRepository(
    PlumbingContext context,
    ILogger<SandCornLinkRepository> logger)
    : PlumbingContextRepository<SandCornLink, SandCornLinkRepository>(context, logger),
      ISandCornLinkRepository
{
    public async Task<Result<List<SandCornLink>>> GetAllWithDetailsAsync()
    {
        try
        {
            List<SandCornLink> list = await _context.SandCornLinks
                .AsNoTracking()
                .Include(p => p.CornEntity)
                .Include(p => p.SandEntity)
                .ToListAsync();

            return list;
        }
        catch (Exception ex)
        {
            return Result.Failure<List<SandCornLink>>(ex.ToString());
        }
    }

    public async Task<Result<List<SandCornLink>>> GetAllWithDetailsAsync(IEnumerable<CornEntity> filter)
    {
        try
        {
            List<long> ids = [.. filter.Select(p => p.Id)];

            List<SandCornLink> list = await _context.SandCornLinks
                .AsNoTracking()
                .Where(e => ids.Contains(e.CornId))
                .Include(p => p.CornEntity)
                .Include(p => p.SandEntity)
                .ToListAsync();

            return list;
        }
        catch (Exception ex)
        {
            return Result.Failure<List<SandCornLink>>(ex.ToString());
        }
    }

    public override async Task<Result<List<SandCornLink>>> GetAllAsync()
    {
        try
        {
            List<SandCornLink> list = await _context.SandCornLinks
                .AsNoTracking()
                .Select(s => new SandCornLink
                {
                    Id = s.Id,
                    SandId = s.SandId,
                    CornId = s.CornId,
                    MatchingPhone = s.MatchingPhone
                })
                .ToListAsync();

            return list;
        }
        catch (Exception ex)
        {
            return Result.Failure<List<SandCornLink>>(ex.ToString());
        }
    }

    public override async Task<Result<List<SandCornLink>>> UpsertRangeAsync(List<SandCornLink> entities)
    {
        if (entities.Count == 0)
            return Result.Success(new List<SandCornLink>());

        // Deduplicate in-memory by (SubsId, CornId)
        List<SandCornLink> uniqueEntities =
        [
            .. entities
                .GroupBy(e => (e.SandId, e.CornId))
                .Select(g => g.Last())
        ];

        int batchSize = 200;
        const int minBatchSize = 1;
        int stagedCount = 0;
        int skipped = 0;

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            await _context.Database.ExecuteSqlRawAsync("""
                CREATE TEMP TABLE IF NOT EXISTS temp_subs_corn_links (
                    SubsId INTEGER NOT NULL,
                    CornId INTEGER NOT NULL,
                    MatchingPhone INTEGER NOT NULL,
                    PRIMARY KEY (SubsId, CornId)
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
                        nameof(SandCornLink),
                        batchSize);

                    if (batchSize == minBatchSize)
                    {
                        var row = batch[0];
                        _logger.LogError(
                            "{Entity} row insert failed: SubsId={SubsId}, CornId={CornId}, MatchingPhone={MatchingPhone}",
                            nameof(SandCornLink),
                            row.SandId,
                            row.CornId,
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

            // Update existing rows
            int updated = await _context.Database.ExecuteSqlRawAsync("""
                UPDATE SubsCornLinks
                SET MatchingPhone = (
                    SELECT t.MatchingPhone
                    FROM temp_subs_corn_links t
                    WHERE t.SubsId = SubsCornLinks.SubsId
                      AND t.CornId = SubsCornLinks.CornId
                )
                WHERE EXISTS (
                    SELECT 1
                    FROM temp_subs_corn_links t
                    WHERE t.SubsId = SubsCornLinks.SubsId
                      AND t.CornId = SubsCornLinks.CornId
                );
            """);

            // Insert missing rows
            int inserted = await _context.Database.ExecuteSqlRawAsync("""
                INSERT INTO SubsCornLinks (SubsId, CornId, MatchingPhone)
                SELECT t.SubsId, t.CornId, t.MatchingPhone
                FROM temp_subs_corn_links t
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM SubsCornLinks s
                    WHERE s.SubsId = t.SubsId
                      AND s.CornId = t.CornId
                );
            """);

            await _context.Database.ExecuteSqlRawAsync("DELETE FROM temp_subs_corn_links;");
            await transaction.CommitAsync();

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Updated={Updated}, Inserted={Inserted}, Skipped={Skipped}",
                nameof(SandCornLink),
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
                nameof(SandCornLink));
            return Result.Failure<List<SandCornLink>>(ex.Message);
        }

        void InsertBatch(List<SandCornLink> batch)
        {
            var sql = new StringBuilder();
            sql.Append("INSERT INTO temp_subs_corn_links VALUES ");

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];
                sql.Append($"({e.SandId}, {e.CornId}, {e.MatchingPhone})");

                if (i < batch.Count - 1)
                    sql.Append(", ");
            }

            sql.Append(';');
            _context.Database.ExecuteSqlRaw(sql.ToString());
        }
    }

    public async Task<Result<List<SandCornLink>>> GetAllAsync(IEnumerable<CornEntity> filter)
    {
        try
        {
            List<long> ids = [.. filter.Select(p => p.Id)];

            List<SandCornLink> set = await _set
                .Where(e => ids.Contains(e.CornId))
                .ToListAsync();

            return Result.Success(set);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<SandCornLink>>(ex.Message);
        }
    }
}
