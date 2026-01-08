using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public class SubsCornLinkRepository(
    PlumbingContext context,
    ILogger<SubsCornLinkRepository> logger)
    : PlumbingContextRepository<SubsCornLink, SubsCornLinkRepository>(context, logger),
      ISubsCornLinkRepository
{
    public async Task<Result<List<SubsCornLink>>> GetAllWithDetailsAsync()
    {
        try
        {
            List<SubsCornLink> list = await _context.SubsCornLinks
                .AsNoTracking()
                .Include(p => p.CornEntity)
                .Include(p => p.SubsEntity)
                .ToListAsync();

            return list;
        }
        catch (Exception ex)
        {
            return Result.Failure<List<SubsCornLink>>(ex.ToString());
        }
    }

    public async Task<Result<List<SubsCornLink>>> GetAllWithDetailsAsync(IEnumerable<CornEntity> filter)
    {
        try
        {
            List<long> ids = [.. filter.Select(p => p.Id)];

            List<SubsCornLink> list = await _context.SubsCornLinks
                .AsNoTracking()
                .Where(e => ids.Contains(e.CornId))
                .Include(p => p.CornEntity)
                .Include(p => p.SubsEntity)
                .ToListAsync();

            return list;
        }
        catch (Exception ex)
        {
            return Result.Failure<List<SubsCornLink>>(ex.ToString());
        }
    }

    public override async Task<Result<List<SubsCornLink>>> GetAllAsync()
    {
        try
        {
            List<SubsCornLink> list = await _context.SubsCornLinks
                .AsNoTracking()
                .Select(s => new SubsCornLink
                {
                    Id = s.Id,
                    SubsId = s.SubsId,
                    CornId = s.CornId,
                    MatchingPhone = s.MatchingPhone
                })
                .ToListAsync();

            return list;
        }
        catch (Exception ex)
        {
            return Result.Failure<List<SubsCornLink>>(ex.ToString());
        }
    }

    public override async Task<Result<List<SubsCornLink>>> UpsertRangeAsync(List<SubsCornLink> entities)
    {
        if (entities.Count == 0)
            return Result.Success(new List<SubsCornLink>());

        // Deduplicate in-memory by (SubsId, CornId)
        List<SubsCornLink> uniqueEntities =
        [
            .. entities
                .GroupBy(e => (e.SubsId, e.CornId))
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
                        nameof(SubsCornLink),
                        batchSize);

                    if (batchSize == minBatchSize)
                    {
                        var row = batch[0];
                        _logger.LogError(
                            "{Entity} row insert failed: SubsId={SubsId}, CornId={CornId}, MatchingPhone={MatchingPhone}",
                            nameof(SubsCornLink),
                            row.SubsId,
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
                nameof(SubsCornLink),
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
                nameof(SubsCornLink);
            return Result.Failure<List<SubsCornLink>>(ex.Message);
        }

        void InsertBatch(List<SubsCornLink> batch)
        {
            var sql = new StringBuilder();
            sql.Append("INSERT INTO temp_subs_corn_links VALUES ");

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];
                sql.Append($"({e.SubsId}, {e.CornId}, {e.MatchingPhone})");

                if (i < batch.Count - 1)
                    sql.Append(", ");
            }

            sql.Append(';');
            _context.Database.ExecuteSqlRaw(sql.ToString());
        }
    }

    public async Task<Result<List<SubsCornLink>>> GetAllAsync(IEnumerable<CornEntity> filter)
    {
        try
        {
            List<long> ids = [.. filter.Select(p => p.Id)];

            List<SubsCornLink> set = await _set
                .Where(e => ids.Contains(e.CornId))
                .ToListAsync();

            return Result.Success(set);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<SubsCornLink>>(ex.Message);
        }
    }
}
