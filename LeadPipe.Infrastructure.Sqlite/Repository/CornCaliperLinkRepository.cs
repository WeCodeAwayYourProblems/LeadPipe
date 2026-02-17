using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class CornCaliperLinkRepository
    (
        PlumbingContext context,
        ILogger<CornCaliperLinkRepository> logger
    ) : PlumbingContextRepository<CornCaliperLink, CornCaliperLinkRepository>(context, logger), IRepository<CornCaliperLink>
{
    protected override IQueryable<CornCaliperLink> WithIncludes(IQueryable<CornCaliperLink> q)
    {
        return q
            .Include(c => c.CornEntity)
            .Include(c => c.CaliperEntity);
    }

    public override async Task<Result<List<CornCaliperLink>>> UpsertRangeAsync(
        List<CornCaliperLink> entities,
        CancellationToken ct = default)
    {
        if (entities.Count == 0)
            return Result.Success(new List<CornCaliperLink>());

        AssertNotString<CornCaliperLink>(nameof(CornCaliperLink.CornId));
        AssertNotString<CornCaliperLink>(nameof(CornCaliperLink.CaliperId));
        AssertNotString<CornCaliperLink>(nameof(CornCaliperLink.MatchingPhone));
        AssertNotString<CornCaliperLink>(nameof(CornCaliperLink.UnixMatchDate));

        int batchSize = 1000;
        const int minBatchSize = 1;
        int stagedCount = 0;
        int skipped = 0;
        const string tempTable = "temp_corn_caliper_links";

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            // 1. Create staging table WITHOUT a Primary Key to allow duplicates initially
            await _context.Database.ExecuteSqlRawAsync($"""
                CREATE TEMP TABLE IF NOT EXISTS {tempTable} (
                    {nameof(CornCaliperLink.CornId)} INTEGER,
                    {nameof(CornCaliperLink.CaliperId)} INTEGER,
                    {nameof(CornCaliperLink.MatchingPhone)} INTEGER,
                    {nameof(CornCaliperLink.UnixMatchDate)} INTEGER
                );
                CREATE INDEX IF NOT EXISTS idx_temp_sync ON {tempTable} 
                    ({nameof(CornCaliperLink.CornId)}, {nameof(CornCaliperLink.CaliperId)}, {nameof(CornCaliperLink.MatchingPhone)});
                DELETE FROM {tempTable};
            """, ct);

            int index = 0;

            while (index < entities.Count)
            {
                int take = Math.Min(batchSize, entities.Count - index);
                var batch = entities.GetRange(index, take);

                try
                {
                    InsertBatch(batch);
                    stagedCount += batch.Count;
                    index += take;

                    if (batchSize < 1000)
                        batchSize = Math.Min(batchSize * 2, 1000);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "{Entity} batch insert failed (size={BatchSize}). Reducing batch size.",
                        nameof(CornCaliperLink),
                        batchSize);

                    if (batchSize == minBatchSize)
                    {
                        var row = batch[0];
                        _logger.LogError(
                            "{Entity} row insert failed: CornId={CornId}, CaliperId={CaliperId}",
                            nameof(CornCaliperLink),
                            row.CornId,
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

            // Perform single-pass bulk UPSERT
            // Groups by the IDs and Phone, then picks the earliest Date
            int totalAffected = await _context.Database.ExecuteSqlRawAsync($"""
                INSERT INTO {TableNames.CornCaliperLinksName} 
                (
                    {nameof(CornCaliperLink.CornId)}, 
                    {nameof(CornCaliperLink.CaliperId)}, 
                    {nameof(CornCaliperLink.MatchingPhone)}, 
                    {nameof(CornCaliperLink.UnixMatchDate)}
                )
                SELECT 
                    {nameof(CornCaliperLink.CornId)}, 
                    {nameof(CornCaliperLink.CaliperId)}, 
                    {nameof(CornCaliperLink.MatchingPhone)}, 
                    MIN({nameof(CornCaliperLink.UnixMatchDate)})
                FROM {tempTable}
                WHERE {nameof(CornCaliperLink.MatchingPhone)} <> 0
                GROUP BY 
                    {nameof(CornCaliperLink.CornId)}, 
                    {nameof(CornCaliperLink.CaliperId)}, 
                    {nameof(CornCaliperLink.MatchingPhone)}
                ON CONFLICT({nameof(CornCaliperLink.CornId)}, {nameof(CornCaliperLink.CaliperId)}) DO UPDATE SET
                    {nameof(CornCaliperLink.MatchingPhone)} = excluded.{nameof(CornCaliperLink.MatchingPhone)},
                    {nameof(CornCaliperLink.UnixMatchDate)} = CASE 
                        WHEN excluded.{nameof(CornCaliperLink.UnixMatchDate)} < {TableNames.CornCaliperLinksName}.{nameof(CornCaliperLink.UnixMatchDate)} 
                        THEN excluded.{nameof(CornCaliperLink.UnixMatchDate)} 
                        ELSE {TableNames.CornCaliperLinksName}.{nameof(CornCaliperLink.UnixMatchDate)} 
                    END;
            """, ct);

            await _context.Database.ExecuteSqlRawAsync($"DELETE FROM {tempTable};", ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Staged={Staged}, Affected={Affected}, Skipped={Skipped}",
                nameof(CornCaliperLink),
                entities.Count,
                stagedCount,
                totalAffected,
                skipped);

            return Result.Success(entities);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Entity} upsert failed.",
                nameof(CornCaliperLink));
            return Result.Failure<List<CornCaliperLink>>(ex.Message);
        }

        void InsertBatch(List<CornCaliperLink> batch)
        {
            var values = new List<object>();
            var rows = new List<string>();

            for (int i = 0; i < batch.Count; i++)
            {
                int offset = i * 4;
                rows.Add($"({{{offset}}}, {{{offset + 1}}}, {{{offset + 2}}}, {{{offset + 3}}})");

                var e = batch[i];
                values.Add(e.CornId);
                values.Add(e.CaliperId);
                values.Add(e.MatchingPhone);
                values.Add(e.UnixMatchDate);
            }

            string sql = $"INSERT INTO {tempTable} VALUES {string.Join(", ", rows)};";
            _context.Database.ExecuteSqlRaw(sql, [.. values]);
        }

    }

}