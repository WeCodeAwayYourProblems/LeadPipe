using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class CornPlumbingLinkRepository
    (
        PlumbingContext context,
        ILogger<CornPlumbingLinkRepository> logger
    ) : PlumbingContextRepository<CornPlumbingLink, CornPlumbingLinkRepository>(context, logger), IRepository<CornPlumbingLink>
{
    protected override IQueryable<CornPlumbingLink> WithIncludes(IQueryable<CornPlumbingLink> q)
    {
        return q
            .Include(q => q.CornEntity)
            .Include(q => q.PlumbingEntity);
    }

    public override async Task<Result<List<CornPlumbingLink>>> UpsertRangeAsync(List<CornPlumbingLink> entities, CancellationToken ct = default)
    {
        if (entities.Count == 0)
            return Result.Success(new List<CornPlumbingLink>());

        AssertNotString<CornPlumbingLink>(nameof(CornPlumbingLink.CornId));
        AssertNotString<CornPlumbingLink>(nameof(CornPlumbingLink.PlumbingId));
        AssertNotString<CornPlumbingLink>(nameof(CornPlumbingLink.MatchingPhone));
        AssertNotString<CornPlumbingLink>(nameof(CornPlumbingLink.UnixMatchDate));

        // Deduplicate by (CornId, PlumbingId)
        List<CornPlumbingLink> uniqueEntities =
        [
            .. entities
                .GroupBy(e => (e.CornId, e.PlumbingId)) // Order matters here
                .Select(g => g.Last())
        ];

        int batchSize = 200;
        const int minBatchSize = 1;
        int stagedCount = 0;
        int skipped = 0;
        const string tempTable = "temp_corn_plumbing_links";

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            // Temp table
            await _context.Database.ExecuteSqlRawAsync($"""
                CREATE TEMP TABLE IF NOT EXISTS {tempTable} (
                    {nameof(CornPlumbingLink.CornId)} INTEGER NOT NULL,
                    {nameof(CornPlumbingLink.PlumbingId)} INTEGER NOT NULL,
                    {nameof(CornPlumbingLink.MatchingPhone)} INTEGER NOT NULL,
                    {nameof(CornPlumbingLink.UnixMatchDate)} INTEGER NOT NULL,
                    PRIMARY KEY ({nameof(CornPlumbingLink.CornId)}, {nameof(CornPlumbingLink.PlumbingId)})
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
                        nameof(CornPlumbingLink),
                        batchSize);

                    if (batchSize == minBatchSize)
                    {
                        var row = batch[0];
                        _logger.LogError(
                            "{Entity} row insert failed: CornId={CornId}, PlumbingId={PlumbingId}",
                            nameof(CornPlumbingLink),
                            row.CornId,
                            row.PlumbingId);

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

            // perform single-pass bulk upsert
            int totalAffected = await _context.Database.ExecuteSqlRawAsync($"""
            INSERT INTO {TableNames.CornPlumbingLinksName}
                ({nameof(CornPlumbingLink.CornId)}, {nameof(CornPlumbingLink.PlumbingId)}, {nameof(CornPlumbingLink.MatchingPhone)}, {nameof(CornPlumbingLink.UnixMatchDate)})
            SELECT 
                {nameof(CornPlumbingLink.CornId)}, {nameof(CornPlumbingLink.PlumbingId)}, {nameof(CornPlumbingLink.MatchingPhone)}, {nameof(CornPlumbingLink.UnixMatchDate)}
            FROM {tempTable}
            ON CONFLICT({nameof(CornPlumbingLink.CornId)}, {nameof(CornPlumbingLink.PlumbingId)}) DO UPDATE SET
                {nameof(CornPlumbingLink.MatchingPhone)} = excluded.{nameof(CornPlumbingLink.MatchingPhone)},
                {nameof(CornPlumbingLink.UnixMatchDate)} = excluded.{nameof(CornPlumbingLink.UnixMatchDate)};
            """, ct);

            await _context.Database.ExecuteSqlRawAsync($"DELETE FROM {tempTable};", ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Affected={Affected}, Skipped={Skipped}",
                nameof(CornPlumbingLink),
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
                nameof(CornPlumbingLink));
            return Result.Failure<List<CornPlumbingLink>>(ex.Message);
        }

        void InsertBatch(List<CornPlumbingLink> batch)
        {
            var values = new List<object>();
            var rows = new List<string>();

            for (int i = 0; i < batch.Count; i++)
            {
                int offset = i * 4;
                rows.Add($"({{{offset}}}, {{{offset + 1}}}, {{{offset + 2}}}, {{{offset + 3}}})");

                var e = batch[i];
                values.Add(e.CornId);
                values.Add(e.PlumbingId);
                values.Add(e.MatchingPhone);
                values.Add(e.UnixMatchDate);
            }

            string sql = $"INSERT INTO {tempTable} VALUES {string.Join(", ", rows)};";
            _context.Database.ExecuteSqlRaw(sql, [.. values]);
        }
    }

}
