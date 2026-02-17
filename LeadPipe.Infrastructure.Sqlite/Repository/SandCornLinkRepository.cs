using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class SandCornLinkRepository(
    PlumbingContext context,
    ILogger<SandCornLinkRepository> logger)
        : PlumbingContextRepository<SandCornLink, SandCornLinkRepository>(context, logger), IRepository<SandCornLink>
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
        AssertNotString<SandCornLink>(nameof(SandCornLink.UnixMatchDate));

        // Deduplicate in-memory by (Sand, CornId)
        List<SandCornLink> uniqueEntities =
        [
            .. entities
                .GroupBy(e => (e.SandId, e.CornId)) // Order matters here
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
                    {nameof(SandCornLink.UnixMatchDate)} INTEGER NOT NULL,
                    PRIMARY KEY ({nameof(SandCornLink.SandId)}, {nameof(SandCornLink.CornId)})
                ) WITHOUT ROWID;
                DELETE FROM {tempTable};
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
                    _logger.LogError(
                        ex,
                        "{Entity} batch insert failed (size={BatchSize}). Reducing batch size.",
                        nameof(SandCornLink),
                        batchSize);

                    if (batchSize == minBatchSize)
                    {
                        var row = batch[0];
                        _logger.LogError(
                            "{Entity} row insert failed: SandId={SandId}, CornId={CornId}",
                            nameof(SandCornLink),
                            row.SandId,
                            row.CornId);

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
            int totalAffected = await _context.Database.ExecuteSqlRawAsync($"""
                INSERT INTO {TableNames.SandCornLinksName}
                    ({nameof(SandCornLink.SandId)}, {nameof(SandCornLink.CornId)}, {nameof(SandCornLink.MatchingPhone)}, {nameof(SandCornLink.UnixMatchDate)})
                SELECT 
                    {nameof(SandCornLink.SandId)}, {nameof(SandCornLink.CornId)}, {nameof(SandCornLink.MatchingPhone)}, {nameof(SandCornLink.UnixMatchDate)}
                FROM {tempTable}
                ON CONFLICT({nameof(SandCornLink.SandId)}, {nameof(SandCornLink.CornId)}) DO UPDATE SET
                    {nameof(SandCornLink.MatchingPhone)} = excluded.{nameof(SandCornLink.MatchingPhone)},
                    {nameof(SandCornLink.UnixMatchDate)} = excluded.{nameof(SandCornLink.UnixMatchDate)};
            """, ct);

            await _context.Database.ExecuteSqlRawAsync($"DELETE FROM {tempTable};", cancellationToken: ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Affected={Affected}, Skipped={Skipped}",
                nameof(SandCornLink),
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
            _logger.LogError(ex, "{Entity} upsert failed.",
                nameof(SandCornLink));
            return Result.Failure<List<SandCornLink>>(ex.ToString());
        }

        void InsertBatch(List<SandCornLink> batch)
        {
            var values = new List<object>();
            var rows = new List<string>();

            for (int i = 0; i < batch.Count; i++)
            {
                int offset = i * 4;
                rows.Add($"({{{offset}}}, {{{offset + 1}}}, {{{offset + 2}}}, {{{offset + 3}}})");

                var e = batch[i];
                values.Add(e.SandId);
                values.Add(e.CornId);
                values.Add(e.MatchingPhone);
                values.Add(e.UnixMatchDate);
            }

            string sql = $"INSERT INTO {tempTable} VALUES {string.Join(", ", rows)};";
            _context.Database.ExecuteSqlRaw(sql, [.. values]);
        }
    }

}
