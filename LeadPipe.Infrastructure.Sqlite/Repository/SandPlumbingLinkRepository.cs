using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public class SandPlumbingLinkRepository(PlumbingContext context, ILogger<SandPlumbingLinkRepository> logger)
    : PlumbingContextRepository<SandPlumbingLink, SandPlumbingLinkRepository>(context, logger), IRepository<SandPlumbingLink>
{
    protected override IQueryable<SandPlumbingLink> WithIncludes(IQueryable<SandPlumbingLink> q)
    {
        return q
            .Include(c => c.SandEntity)
            .Include(c => c.PlumbingEntity);
    }
    
    public override async Task<Result<List<SandPlumbingLink>>> UpsertRangeAsync(List<SandPlumbingLink> entities, CancellationToken ct = default)
    {
        if (entities.Count == 0)
            return Result.Success(new List<SandPlumbingLink>());

        AssertNotString<SandPlumbingLink>(nameof(SandPlumbingLink.SandId));
        AssertNotString<SandPlumbingLink>(nameof(SandPlumbingLink.PlumbingId));
        AssertNotString<SandPlumbingLink>(nameof(SandPlumbingLink.MatchingPhone));
        AssertNotString<SandPlumbingLink>(nameof(SandPlumbingLink.UnixMatchDate));

        // Deduplicate in-memory
        List<SandPlumbingLink> uniqueEntities =
        [
            .. entities
                .GroupBy(e => (e.SandId, e.PlumbingId)) // Order matters here
                .Select(g => g.Last())
        ];

        int batchSize = 200;
        const int minBatchSize = 1;
        int stagedCount = 0;
        int skipped = 0;
        const string tempTable = "temp_sand_plumbing_links";

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            // Temp table for staging
            await _context.Database.ExecuteSqlRawAsync($"""
                CREATE TEMP TABLE IF NOT EXISTS {tempTable} (
                    {nameof(SandPlumbingLink.SandId)} INTEGER NOT NULL,
                    {nameof(SandPlumbingLink.PlumbingId)} INTEGER NOT NULL,
                    {nameof(SandPlumbingLink.MatchingPhone)} INTEGER NOT NULL,
                    {nameof(SandPlumbingLink.UnixMatchDate)} INTEGER NOT NULL,
                    PRIMARY KEY ({nameof(SandPlumbingLink.SandId)}, {nameof(SandPlumbingLink.PlumbingId)})
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
                       nameof(SandPlumbingLink),
                       batchSize);

                    if (batchSize == minBatchSize)
                    {
                        var row = batch[0];
                        _logger.LogError(
                            "{Entity} row insert failed: SandId={SandId}, PlumbingId={PlumbingId}",
                            nameof(SandPlumbingLink),
                            row.SandId,
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

            // ---- Phase 1: UPDATE existing rows ----
            int totalAffected = await _context.Database.ExecuteSqlRawAsync($"""
                INSERT INTO {TableNames.SandPlumbingLinksName}
                    ({nameof(SandPlumbingLink.SandId)}, {nameof(SandPlumbingLink.PlumbingId)}, {nameof(SandPlumbingLink.MatchingPhone)}, {nameof(SandPlumbingLink.UnixMatchDate)})
                SELECT 
                    {nameof(SandPlumbingLink.SandId)}, {nameof(SandPlumbingLink.PlumbingId)}, {nameof(SandPlumbingLink.MatchingPhone)}, {nameof(SandPlumbingLink.UnixMatchDate)}
                FROM {tempTable}
                ON CONFLICT({nameof(SandPlumbingLink.SandId)}, {nameof(SandPlumbingLink.PlumbingId)}) DO UPDATE SET
                    {nameof(SandPlumbingLink.MatchingPhone)} = excluded.{nameof(SandPlumbingLink.MatchingPhone)},
                    {nameof(SandPlumbingLink.UnixMatchDate)} = excluded.{nameof(SandPlumbingLink.UnixMatchDate)};
            """, ct);

            await _context.Database.ExecuteSqlRawAsync($"DELETE FROM {tempTable};", cancellationToken: ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Affected={Affected}, Skipped={Skipped}",
                nameof(SandPlumbingLink),
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
                nameof(SandPlumbingLink));
            return Result.Failure<List<SandPlumbingLink>>(ex.Message);
        }

        void InsertBatch(List<SandPlumbingLink> batch)
        {
            var values = new List<object>();
            var rows = new List<string>();

            for (int i = 0; i < batch.Count; i++)
            {
                int offset = i * 4;
                rows.Add($"({{{offset}}}, {{{offset + 1}}}, {{{offset + 2}}}, {{{offset + 3}}})");

                var e = batch[i];
                values.Add(e.SandId);
                values.Add(e.PlumbingId);
                values.Add(e.MatchingPhone);
                values.Add(e.UnixMatchDate);
            }

            string sql = $"INSERT INTO {tempTable} VALUES {string.Join(", ", rows)};";
            _context.Database.ExecuteSqlRaw(sql, [.. values]);
        }
    }

}
