using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class CustardCaliperLinkRepository
    (
        PlumbingContext context,
        ILogger<CustardCaliperLinkRepository> logger
    ) : PlumbingContextRepository<CustardCaliperLink, CustardCaliperLinkRepository>(context, logger), IRepository<CustardCaliperLink>
{
    protected override IQueryable<CustardCaliperLink> WithIncludes(IQueryable<CustardCaliperLink> q)
    {
        return q
            .Include(q => q.Custard)
            .Include(q => q.Caliper);
    }

    public override async Task<Result<List<CustardCaliperLink>>> UpsertRangeAsync(List<CustardCaliperLink> entities, CancellationToken ct = default)
    {
        if (entities.Count == 0)
            return Result.Success(new List<CustardCaliperLink>());

        AssertNotString<CustardCaliperLink>(nameof(CustardCaliperLink.CustardId));
        AssertNotString<CustardCaliperLink>(nameof(CustardCaliperLink.CaliperId));
        AssertNotString<CustardCaliperLink>(nameof(CustardCaliperLink.MatchingPhone));
        AssertNotString<CustardCaliperLink>(nameof(CustardCaliperLink.UnixMatchDate));

        // Deduplicate by (CustardId, CaliperId)
        List<CustardCaliperLink> uniqueEntities =
        [
            .. entities
                .GroupBy(e => (e.CustardId, e.CaliperId)) // Order matters here
                .Select(g => g.Last())
        ];

        int batchSize = 200;
        const int minBatchSize = 1;
        int stagedCount = 0;
        int skipped = 0;
        const string tempTable = "temp_custard_caliper_links";

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            // Temp table
            await _context.Database.ExecuteSqlRawAsync($"""
                CREATE TEMP TABLE IF NOT EXISTS {tempTable} (
                    {nameof(CustardCaliperLink.CustardId)} INTEGER NOT NULL,
                    {nameof(CustardCaliperLink.CaliperId)} INTEGER NOT NULL,
                    {nameof(CustardCaliperLink.MatchingPhone)} INTEGER NOT NULL,
                    {nameof(CustardCaliperLink.UnixMatchDate)} INTEGER NOT NULL,
                    PRIMARY KEY ({nameof(CustardCaliperLink.CustardId)}, {nameof(CustardCaliperLink.CaliperId)})
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
                        nameof(CustardCaliperLink),
                        batchSize);

                    if (batchSize == minBatchSize)
                    {
                        var row = batch[0];
                        _logger.LogError(
                            "{Entity} row insert failed: CustardId={CustardId}, CaliperId={CaliperId}",
                            nameof(CustardCaliperLink),
                            row.CustardId,
                            row.CaliperId);

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

            // Target index: custardCaliper.HasIndex(l => new { l.CustardId, l.CaliperId }).IsUnique();
            int totalAffected = await _context.Database.ExecuteSqlRawAsync($"""
                INSERT INTO {TableNames.CustardCaliperLinksName}
                    ({nameof(CustardCaliperLink.CustardId)}, {nameof(CustardCaliperLink.CaliperId)}, {nameof(CustardCaliperLink.MatchingPhone)}, {nameof(CustardCaliperLink.UnixMatchDate)})
                SELECT 
                    {nameof(CustardCaliperLink.CustardId)}, {nameof(CustardCaliperLink.CaliperId)}, {nameof(CustardCaliperLink.MatchingPhone)}, {nameof(CustardCaliperLink.UnixMatchDate)}
                FROM {tempTable}
                ON CONFLICT({nameof(CustardCaliperLink.CustardId)}, {nameof(CustardCaliperLink.CaliperId)}) DO UPDATE SET
                    {nameof(CustardCaliperLink.MatchingPhone)} = excluded.{nameof(CustardCaliperLink.MatchingPhone)},
                    {nameof(CustardCaliperLink.UnixMatchDate)} = excluded.{nameof(CustardCaliperLink.UnixMatchDate)};
            """, ct);

            await _context.Database.ExecuteSqlRawAsync($"DELETE FROM {tempTable};", ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Affected={Affected}, Skipped={Skipped}",
                nameof(CustardCaliperLink),
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
                nameof(CustardCaliperLink));
            return Result.Failure<List<CustardCaliperLink>>(ex.Message);
        }

        void InsertBatch(List<CustardCaliperLink> batch)
        {
            var values = new List<object>();
            var rows = new List<string>();

            for (int i = 0; i < batch.Count; i++)
            {
                int offset = i * 4;
                rows.Add($"({{{offset}}}, {{{offset + 1}}}, {{{offset + 2}}}, {{{offset + 3}}})");

                var e = batch[i];
                values.Add(e.CustardId);
                values.Add(e.CaliperId);
                values.Add(e.MatchingPhone);
                values.Add(e.UnixMatchDate);
            }

            string sql = $"INSERT INTO {tempTable} VALUES {string.Join(", ", rows)};";
            _context.Database.ExecuteSqlRaw(sql, [.. values]);
        }
    }

}
