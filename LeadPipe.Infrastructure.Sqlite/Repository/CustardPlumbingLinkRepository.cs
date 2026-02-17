using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class CustardPlumbingLinkRepository
    (
        PlumbingContext context,
        ILogger<CustardPlumbingLinkRepository> logger
    ) : PlumbingContextRepository<CustardPlumbingLink, CustardPlumbingLinkRepository>(context, logger), IRepository<CustardPlumbingLink>
{
    protected override IQueryable<CustardPlumbingLink> WithIncludes(IQueryable<CustardPlumbingLink> q)
    {
        return q
            .Include(q => q.Custard)
            .Include(q => q.Plumbing);
    }

    public override async Task<Result<List<CustardPlumbingLink>>> UpsertRangeAsync(List<CustardPlumbingLink> entities, CancellationToken ct = default)
    {
        if (entities.Count == 0)
            return Result.Success(new List<CustardPlumbingLink>());

        AssertNotString<CustardPlumbingLink>(nameof(CustardPlumbingLink.CustardId));
        AssertNotString<CustardPlumbingLink>(nameof(CustardPlumbingLink.PlumbingId));
        AssertNotString<CustardPlumbingLink>(nameof(CustardPlumbingLink.MatchingPhone));
        AssertNotString<CustardPlumbingLink>(nameof(CustardPlumbingLink.UnixMatchDate));

        // Deduplicate by (CustardId, PlumbingId)
        List<CustardPlumbingLink> uniqueEntities =
        [
            .. entities
                .GroupBy(e => (e.CustardId, e.PlumbingId)) // order matters
                .Select(g => g.Last())
        ];

        int batchSize = 200;
        const int minBatchSize = 1;
        int stagedCount = 0;
        int skipped = 0;
        const string tempTable = "temp_custard_plumbing_links";

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            // Temp table
            await _context.Database.ExecuteSqlRawAsync($"""
                CREATE TEMP TABLE IF NOT EXISTS {tempTable} (
                    {nameof(CustardPlumbingLink.CustardId)} INTEGER NOT NULL,
                    {nameof(CustardPlumbingLink.PlumbingId)} INTEGER NOT NULL,
                    {nameof(CustardPlumbingLink.MatchingPhone)} INTEGER NOT NULL,
                    {nameof(CustardPlumbingLink.UnixMatchDate)} INTEGER NOT NULL,
                    PRIMARY KEY ({nameof(CustardPlumbingLink.CustardId)}, {nameof(CustardPlumbingLink.PlumbingId)})
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
                        nameof(CustardPlumbingLink),
                        batchSize);

                    if (batchSize == minBatchSize)
                    {
                        var row = batch[0];
                        _logger.LogError(
                            "{Entity} row insert failed: CustardId={CustardId}, PlumbingId={PlumbingId}",
                            nameof(CustardPlumbingLink),
                            row.CustardId,
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

            // Targets: custardPlumbing.HasIndex(l => new { l.CustardId, l.PlumbingId }).IsUnique();
            int totalAffected = await _context.Database.ExecuteSqlRawAsync($"""
                INSERT INTO {TableNames.CustardPlumbingLinksName}
                    ({nameof(CustardPlumbingLink.CustardId)}, {nameof(CustardPlumbingLink.PlumbingId)}, {nameof(CustardPlumbingLink.MatchingPhone)}, {nameof(CustardPlumbingLink.UnixMatchDate)})
                SELECT 
                    {nameof(CustardPlumbingLink.CustardId)}, {nameof(CustardPlumbingLink.PlumbingId)}, {nameof(CustardPlumbingLink.MatchingPhone)}, {nameof(CustardPlumbingLink.UnixMatchDate)}
                FROM {tempTable}
                ON CONFLICT({nameof(CustardPlumbingLink.CustardId)}, {nameof(CustardPlumbingLink.PlumbingId)}) DO UPDATE SET
                    {nameof(CustardPlumbingLink.MatchingPhone)} = excluded.{nameof(CustardPlumbingLink.MatchingPhone)},
                    {nameof(CustardPlumbingLink.UnixMatchDate)} = excluded.{nameof(CustardPlumbingLink.UnixMatchDate)};
            """, ct);

            await _context.Database.ExecuteSqlRawAsync($"DELETE FROM {tempTable};", ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Affected={Affected}, Skipped={Skipped}",
                nameof(CustardPlumbingLink),
                entities.Count,
                uniqueEntities.Count,
                stagedCount,
                totalAffected,
                skipped);

            return Result.Success(uniqueEntities);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Entity} upsert failed.",
                nameof(CustardPlumbingLink));
            return Result.Failure<List<CustardPlumbingLink>>(ex.Message);
        }

        void InsertBatch(List<CustardPlumbingLink> batch)
        {
            var values = new List<object>();
            var rows = new List<string>();

            for (int i = 0; i < batch.Count; i++)
            {
                int offset = i * 4;
                rows.Add($"({{{offset}}}, {{{offset + 1}}}, {{{offset + 2}}}, {{{offset + 3}}})");

                var e = batch[i];
                values.Add(e.CustardId);
                values.Add(e.PlumbingId);
                values.Add(e.MatchingPhone);
                values.Add(e.UnixMatchDate);
            }

            string sql = $"INSERT INTO {tempTable} VALUES {string.Join(", ", rows)};";
            _context.Database.ExecuteSqlRaw(sql, [.. values]);
        }
    }

}
