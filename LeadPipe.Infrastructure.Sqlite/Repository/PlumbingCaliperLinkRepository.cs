using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class PlumbingCaliperLinkRepository
    (
        PlumbingContext context,
        ILogger<PlumbingCaliperLinkRepository> logger
    ) : PlumbingContextRepository<PlumbingCaliperLink, PlumbingCaliperLinkRepository>(context, logger), IRepository<PlumbingCaliperLink>
{
    protected override IQueryable<PlumbingCaliperLink> WithIncludes(IQueryable<PlumbingCaliperLink> q)
    {
        return q
            .Include(c => c.PlumbingEntity)
            .Include(c => c.CaliperEntity);
    }

    public override async Task<Result<List<PlumbingCaliperLink>>> UpsertRangeAsync(List<PlumbingCaliperLink> entities, CancellationToken ct = default)
    {
        if (entities.Count == 0)
            return Result.Success<List<PlumbingCaliperLink>>([]);

        AssertNotString<PlumbingCaliperLink>(nameof(PlumbingCaliperLink.PlumbingId));
        AssertNotString<PlumbingCaliperLink>(nameof(PlumbingCaliperLink.CaliperId));
        AssertNotString<PlumbingCaliperLink>(nameof(PlumbingCaliperLink.MatchingPhone));
        AssertNotString<PlumbingCaliperLink>(nameof(PlumbingCaliperLink.UnixMatchDate));

        // Deduplicate in-memory by (PlumbingId, CaliperId)
        List<PlumbingCaliperLink> uniqueEntities =
        [
            .. entities
            .GroupBy(e => (e.PlumbingId, e.CaliperId))
            .Select(g => g.Last())
        ];

        int batchSize = 200;
        const int minBatchSize = 1;
        int stagedCount = 0;
        int skipped = 0;
        const string tempTable = "temp_plumbing_caliper_links";

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            await _context.Database.ExecuteSqlRawAsync($"""
                CREATE TEMP TABLE IF NOT EXISTS {tempTable} (
                    {nameof(PlumbingCaliperLink.PlumbingId)} INTEGER NOT NULL,
                    {nameof(PlumbingCaliperLink.CaliperId)} INTEGER NOT NULL,
                    {nameof(PlumbingCaliperLink.MatchingPhone)} INTEGER NOT NULL,
                    {nameof(PlumbingCaliperLink.UnixMatchDate)} INTEGER NOT NULL,
                    PRIMARY KEY ({nameof(PlumbingCaliperLink.PlumbingId)}, {nameof(PlumbingCaliperLink.CaliperId)})
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
                        nameof(PlumbingCaliperLink),
                        batchSize);

                    if (batchSize == minBatchSize)
                    {
                        var row = batch[0];
                        _logger.LogError(
                            "{Entity} row insert failed: PlumbingId={PlumbingId}, CaliperId={CaliperId}",
                            nameof(PlumbingCaliperLink),
                            row.PlumbingId,
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

            // Target index: plumbCaliper.HasIndex(l => new { l.PlumbingId, l.CaliperId }).IsUnique();
            int totalAffected = await _context.Database.ExecuteSqlRawAsync($"""
                INSERT INTO {TableNames.PlumbingCaliperLinksName}
                    ({nameof(PlumbingCaliperLink.PlumbingId)}, {nameof(PlumbingCaliperLink.CaliperId)}, {nameof(PlumbingCaliperLink.MatchingPhone)}, {nameof(PlumbingCaliperLink.UnixMatchDate)})
                SELECT 
                    {nameof(PlumbingCaliperLink.PlumbingId)}, {nameof(PlumbingCaliperLink.CaliperId)}, {nameof(PlumbingCaliperLink.MatchingPhone)}, {nameof(PlumbingCaliperLink.UnixMatchDate)}
                FROM {tempTable}
                ON CONFLICT({nameof(PlumbingCaliperLink.PlumbingId)}, {nameof(PlumbingCaliperLink.CaliperId)}) DO UPDATE SET
                    {nameof(PlumbingCaliperLink.MatchingPhone)} = excluded.{nameof(PlumbingCaliperLink.MatchingPhone)},
                    {nameof(PlumbingCaliperLink.UnixMatchDate)} = excluded.{nameof(PlumbingCaliperLink.UnixMatchDate)};
            """, ct);

            await _context.Database.ExecuteSqlRawAsync($"DELETE FROM {tempTable};", cancellationToken: ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Affected={Affected}, Skipped={Skipped}",
                nameof(PlumbingCaliperLink),
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
            _logger.LogError(ex, "{Entity} upsert failed. Exception Message: {Message}",
                nameof(PlumbingCaliperLink),
                ex.Message);
            return Result.Failure<List<PlumbingCaliperLink>>(ex.Message);
        }

        void InsertBatch(List<PlumbingCaliperLink> batch)
        {
            var values = new List<object>();
            var rows = new List<string>();

            for (int i = 0; i < batch.Count; i++)
            {
                int offset = i * 4;
                rows.Add($"({{{offset}}}, {{{offset + 1}}}, {{{offset + 2}}}, {{{offset + 3}}})");

                var e = batch[i];
                values.Add(e.PlumbingId);
                values.Add(e.CaliperId);
                values.Add(e.MatchingPhone);
                values.Add(e.UnixMatchDate);
            }

            string sql = $"INSERT INTO {tempTable} VALUES {string.Join(", ", rows)};";
            _context.Database.ExecuteSqlRaw(sql, [.. values]);
        }
    }

}
