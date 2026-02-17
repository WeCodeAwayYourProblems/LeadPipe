using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class SandCaliperLinkRepository(PlumbingContext context, ILogger<SandCaliperLinkRepository> logger)
    : PlumbingContextRepository<SandCaliperLink, SandCaliperLinkRepository>(context, logger), IRepository<SandCaliperLink>
{
    protected override IQueryable<SandCaliperLink> WithIncludes(IQueryable<SandCaliperLink> q)
    {
        return q
            .Include(c => c.SandEntity)
            .Include(c => c.CaliperEntity);
    }

    public override async Task<Result<List<SandCaliperLink>>> UpsertRangeAsync(List<SandCaliperLink> entities, CancellationToken ct = default)
    {
        if (entities.Count == 0)
            return Result.Success(new List<SandCaliperLink>());

        AssertNotString<SandCaliperLink>(nameof(SandCaliperLink.SandId));
        AssertNotString<SandCaliperLink>(nameof(SandCaliperLink.CaliperId));
        AssertNotString<SandCaliperLink>(nameof(SandCaliperLink.MatchingPhone));
        AssertNotString<SandCaliperLink>(nameof(SandCaliperLink.UnixMatchDate));

        // Deduplicate in-memory by (SandId, CaliperId)
        List<SandCaliperLink> uniqueEntities = 
        [
            .. entities
            .GroupBy(e => (e.SandId, e.CaliperId)) // Order matters
            .Select(g => g.Last())
        ];

        int batchSize = 200;
        const int minBatchSize = 1;
        int stagedCount = 0;
        int skipped = 0;
        const string tempTable = "temp_sand_caliper_links";

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            // Temp table for staging
            await _context.Database.ExecuteSqlRawAsync($"""
                CREATE TEMP TABLE IF NOT EXISTS {tempTable} (
                    {nameof(SandCaliperLink.SandId)} INTEGER NOT NULL,
                    {nameof(SandCaliperLink.CaliperId)} INTEGER NOT NULL,
                    {nameof(SandCaliperLink.MatchingPhone)} INTEGER NOT NULL,
                    {nameof(SandCaliperLink.UnixMatchDate)} INTEGER NOT NULL,
                    PRIMARY KEY ({nameof(SandCaliperLink.SandId)}, {nameof(SandCaliperLink.CaliperId)})
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
                    _logger.LogError(ex, "{Entity} batch insert failed (size={BatchSize}).",
                        nameof(SandCaliperLink),
                        batchSize);

                    if (batchSize == minBatchSize)
                    {
                        var row = batch[0];
                        _logger.LogError(
                            "{Entity} row insert failed: SandId={SandId}, CaliperId={CaliperId}",
                            nameof(SandCaliperLink),
                            row.SandId,
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

            int totalAffected = await _context.Database.ExecuteSqlRawAsync($"""
                INSERT INTO {TableNames.SandCaliperLinksName}
                    ({nameof(SandCaliperLink.SandId)}, {nameof(SandCaliperLink.CaliperId)}, {nameof(SandCaliperLink.MatchingPhone)}, {nameof(SandCaliperLink.UnixMatchDate)})
                SELECT 
                    {nameof(SandCaliperLink.SandId)}, {nameof(SandCaliperLink.CaliperId)}, {nameof(SandCaliperLink.MatchingPhone)}, {nameof(SandCaliperLink.UnixMatchDate)}
                FROM {tempTable}
                ON CONFLICT({nameof(SandCaliperLink.SandId)}, {nameof(SandCaliperLink.CaliperId)}) DO UPDATE SET
                    {nameof(SandCaliperLink.MatchingPhone)} = excluded.{nameof(SandCaliperLink.MatchingPhone)},
                    {nameof(SandCaliperLink.UnixMatchDate)} = excluded.{nameof(SandCaliperLink.UnixMatchDate)};
            """, ct);

            await _context.Database.ExecuteSqlRawAsync($"DELETE FROM {tempTable};", cancellationToken: ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Affected={Affected}, Skipped={Skipped}",
                nameof(SandCaliperLink), 
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
                nameof(SandCaliperLink));
            return Result.Failure<List<SandCaliperLink>>(ex.Message);
        }

        void InsertBatch(List<SandCaliperLink> batch)
        {
            var values = new List<object>();
            var rows = new List<string>();

            for (int i = 0; i < batch.Count; i++)
            {
                int offset = i * 4;
                rows.Add($"({{{offset}}}, {{{offset + 1}}}, {{{offset + 2}}}, {{{offset + 3}}})");

                var e = batch[i];
                values.Add(e.SandId);
                values.Add(e.CaliperId);
                values.Add(e.MatchingPhone);
                values.Add(e.UnixMatchDate);
            }

            string sql = $"INSERT INTO {tempTable} VALUES {string.Join(", ", rows)};";
            _context.Database.ExecuteSqlRaw(sql, [.. values]);
        }
    }

}
