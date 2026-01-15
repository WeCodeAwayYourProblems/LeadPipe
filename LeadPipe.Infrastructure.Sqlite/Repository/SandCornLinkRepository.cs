using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class SandCornLinkRepository(
    PlumbingContext context,
    ILogger<SandCornLinkRepository> logger)
        : PlumbingContextRepository<SandCornLink, SandCornLinkRepository>(context, logger),
      ISandCornLinkRepository
{
    protected override IQueryable<SandCornLink> WithIncludes(IQueryable<SandCornLink> q)
    {
        return q.Include(x => x.CornEntity)
                .Include(x => x.SandEntity);
    }

    public override async Task<Result<List<SandCornLink>>> UpsertRangeAsync(List<SandCornLink> entities, CancellationToken ct = default)
    {
        if (entities.Count == 0)
            return Result.Success(new List<SandCornLink>());

        AssertNotString<SandCornLink>(nameof(SandCornLink.SandId));
        AssertNotString<SandCornLink>(nameof(SandCornLink.CornId));
        AssertNotString<SandCornLink>(nameof(SandCornLink.MatchingPhone));

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
        const string tempTable = "temp_sand_corn_links";

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            await _context.Database.ExecuteSqlRawAsync($"""
                CREATE TEMP TABLE IF NOT EXISTS {tempTable} (
                    {nameof(SandCornLink.SandId)} INTEGER NOT NULL,
                    {nameof(SandCornLink.CornId)} INTEGER NOT NULL,
                    {nameof(SandCornLink.MatchingPhone)} INTEGER NOT NULL,
                    PRIMARY KEY ({nameof(SandCornLink.SandId)}, {nameof(SandCornLink.CornId)})
                ) WITHOUT ROWID;
            """, ct);

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
                            "{Entity} row insert failed: SandId={SandId}, CornId={CornId}, MatchingPhone={MatchingPhone}",
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
            int updated = await _context.Database.ExecuteSqlRawAsync($"""
                UPDATE {TableNames.SandCornLinksName}
                SET {nameof(SandCornLink.MatchingPhone)} = (
                    SELECT t.{nameof(SandCornLink.MatchingPhone)}
                    FROM {tempTable} t
                    WHERE t.{nameof(SandCornLink.SandId)} = {TableNames.SandCornLinksName}.{nameof(SandCornLink.SandId)}
                      AND t.{nameof(SandCornLink.CornId)} = {TableNames.SandCornLinksName}.{nameof(SandCornLink.CornId)}
                )
                WHERE EXISTS (
                    SELECT 1
                    FROM {tempTable} t
                    WHERE t.{nameof(SandCornLink.SandId)} = {TableNames.SandCornLinksName}.{nameof(SandCornLink.SandId)}
                      AND t.{nameof(SandCornLink.CornId)} = {TableNames.SandCornLinksName}.{nameof(SandCornLink.CornId)}
                );
            """, cancellationToken: ct);

            // Insert missing rows
            int inserted = await _context.Database.ExecuteSqlRawAsync($"""
                INSERT INTO {TableNames.SandCornLinksName} ({nameof(SandCornLink.SandId)}, {nameof(SandCornLink.CornId)}, {nameof(SandCornLink.MatchingPhone)})
                SELECT t.{nameof(SandCornLink.SandId)}, t.{nameof(SandCornLink.CornId)}, t.{nameof(SandCornLink.MatchingPhone)}
                FROM {tempTable} t
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM {TableNames.SandCornLinksName} s
                    WHERE s.{nameof(SandCornLink.SandId)} = t.{nameof(SandCornLink.SandId)}
                      AND s.{nameof(SandCornLink.CornId)} = t.{nameof(SandCornLink.CornId)}
                );
            """, cancellationToken: ct);

            await _context.Database.ExecuteSqlRawAsync($"DELETE FROM {tempTable};", cancellationToken: ct);
            await transaction.CommitAsync(ct);

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
            sql.Append($"INSERT INTO {tempTable} VALUES ");

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
}
