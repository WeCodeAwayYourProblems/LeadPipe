using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class CustardCornLinkRepository
    (
        PlumbingContext context,
        ILogger<CustardCornLinkRepository> logger
    ) : PlumbingContextRepository<CustardCornLink, CustardCornLinkRepository>(context, logger), IRepository<CustardCornLink>
{
    protected override IQueryable<CustardCornLink> WithIncludes(IQueryable<CustardCornLink> q)
    {
        return q
            .Include(q => q.Custard)
            .Include(q => q.Corn);
    }

    public override async Task<Result<List<CustardCornLink>>> UpsertRangeAsync(List<CustardCornLink> entities, CancellationToken ct = default)
    {
        if (entities.Count == 0)
            return Result.Success(new List<CustardCornLink>());

        AssertNotString<CustardCornLink>(nameof(CustardCornLink.CustardId));
        AssertNotString<CustardCornLink>(nameof(CustardCornLink.CornId));
        AssertNotString<CustardCornLink>(nameof(CustardCornLink.MatchingPhone));
        AssertNotString<CustardCornLink>(nameof(CustardCornLink.UnixMatchDate));

        // Deduplicate by (CustardId, CornId)
        List<CustardCornLink> uniqueEntities =
        [
            .. entities
                .Where(e => e.MatchingPhone != 0)
                .GroupBy(e => (e.CustardId, e.CornId)) // Order matters here
                .Select(g => g.Last())
        ];

        int batchSize = 200;
        const int minBatchSize = 1;
        int stagedCount = 0;
        int skipped = 0;
        const string tempTable = "temp_custard_corn_links";

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            // Temp table
            await _context.Database.ExecuteSqlRawAsync($"""
                CREATE TEMP TABLE IF NOT EXISTS {tempTable} (
                    {nameof(CustardCornLink.CustardId)} INTEGER NOT NULL,
                    {nameof(CustardCornLink.CornId)} INTEGER NOT NULL,
                    {nameof(CustardCornLink.MatchingPhone)} INTEGER NOT NULL CHECK({nameof(CustardCornLink.MatchingPhone)} <> 0),
                    {nameof(CustardCornLink.UnixMatchDate)} INTEGER NOT NULL,
                    PRIMARY KEY ({nameof(CustardCornLink.CustardId)}, {nameof(CustardCornLink.CornId)})
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
                        "{Entity} batch insert failed (size={BatchSize})",
                        nameof(CustardCornLink),
                        batchSize);

                    if (batchSize == minBatchSize)
                    {
                        var row = batch[0];
                        _logger.LogError(
                            "{Entity} row insert failed: CustardId={CustardId}, CornId={CornId}",
                            nameof(CustardCornLink),
                            row.CustardId,
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

            // We only move records from temp if the parent entities actually exist.
            int totalAffected = await _context.Database.ExecuteSqlRawAsync($"""
                INSERT INTO {TableNames.CustardCornLinksName}
                    ({nameof(CustardCornLink.CustardId)}, {nameof(CustardCornLink.CornId)}, {nameof(CustardCornLink.MatchingPhone)}, {nameof(CustardCornLink.UnixMatchDate)})
                SELECT 
                    t.{nameof(CustardCornLink.CustardId)}, t.{nameof(CustardCornLink.CornId)}, t.{nameof(CustardCornLink.MatchingPhone)}, t.{nameof(CustardCornLink.UnixMatchDate)}
                FROM {tempTable} t
                WHERE EXISTS (SELECT 1 FROM {TableNames.CustardEntitiesName} c WHERE c.Id = t.{nameof(CustardCornLink.CustardId)})
                  AND EXISTS (SELECT 1 FROM {TableNames.CornEntitiesName} r WHERE r.Id = t.{nameof(CustardCornLink.CornId)})
                ON CONFLICT({nameof(CustardCornLink.CustardId)}, {nameof(CustardCornLink.CornId)}) DO UPDATE SET
                    {nameof(CustardCornLink.MatchingPhone)} = excluded.{nameof(CustardCornLink.MatchingPhone)},
                    {nameof(CustardCornLink.UnixMatchDate)} = excluded.{nameof(CustardCornLink.UnixMatchDate)};
            """, ct);

            await _context.Database.ExecuteSqlRawAsync($"DELETE FROM {tempTable};", ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Affected={Affected}, Skipped={Skipped}",
                nameof(CustardCornLink),
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
                nameof(CustardCornLink));
            return Result.Failure<List<CustardCornLink>>(ex.Message);
        }

        void InsertBatch(List<CustardCornLink> batch)
        {
            var values = new List<object>();
            var rows = new List<string>();

            for (int i = 0; i < batch.Count; i++)
            {
                int offset = i * 4;
                rows.Add($"({{{offset}}}, {{{offset + 1}}}, {{{offset + 2}}}, {{{offset + 3}}})");

                var e = batch[i];
                values.Add(e.CustardId);
                values.Add(e.CornId);
                values.Add(e.MatchingPhone);
                values.Add(e.UnixMatchDate);
            }

            string sql = $"INSERT INTO {tempTable} VALUES {string.Join(", ", rows)};";
            _context.Database.ExecuteSqlRaw(sql, [.. values]);
        }
    }

}
