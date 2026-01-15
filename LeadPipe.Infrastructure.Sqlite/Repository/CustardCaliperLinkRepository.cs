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
    ) : PlumbingContextRepository<CustardCaliperLink, CustardCaliperLinkRepository>(context, logger), ICustardCaliperLinkRepository
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

        // Deduplicate by (CornId, PlumbingId)
        List<CustardCaliperLink> uniqueEntities =
        [
            .. entities
                .GroupBy(e => (e.CustardId, e.CaliperId))
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
                    PRIMARY KEY ({nameof(CustardCaliperLink.CustardId)}, {nameof(CustardCaliperLink.CaliperId)})
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
                        nameof(CustardCaliperLink),
                        batchSize);

                    if (batchSize == minBatchSize)
                    {
                        var row = batch[0];
                        _logger.LogError(
                            "{Entity} row insert failed: CornId={CornId}, PlumbingId={PlumbingId}",
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

            // ---- UPDATE existing ----
            await _context.Database.ExecuteSqlRawAsync($"""
                UPDATE {TableNames.CustardCaliperLinksName}
                SET {nameof(CustardCaliperLink.MatchingPhone)} = (
                    SELECT t.{nameof(CustardCaliperLink.MatchingPhone)}
                    FROM {tempTable} t
                    WHERE t.{nameof(CustardCaliperLink.CustardId)} = {TableNames.CustardCaliperLinksName}.{nameof(CustardCaliperLink.CustardId)}
                      AND t.{nameof(CustardCaliperLink.CaliperId)} = {TableNames.CustardCaliperLinksName}.{nameof(CustardCaliperLink.CaliperId)}
                )
                WHERE EXISTS (
                    SELECT 1
                    FROM {tempTable} t
                    WHERE t.{nameof(CustardCaliperLink.CustardId)} = {TableNames.CustardCaliperLinksName}.{nameof(CustardCaliperLink.CustardId)}
                      AND t.{nameof(CustardCaliperLink.CaliperId)} = {TableNames.CustardCaliperLinksName}.{nameof(CustardCaliperLink.CaliperId)}
                );
            """, ct);

            // ---- INSERT missing ----
            int inserted = await _context.Database.ExecuteSqlRawAsync($"""
                INSERT INTO {TableNames.CustardCaliperLinksName}
                    ({nameof(CustardCaliperLink.CustardId)}, {nameof(CustardCaliperLink.CaliperId)}, {nameof(CustardCaliperLink.MatchingPhone)})
                SELECT
                    t.{nameof(CustardCaliperLink.CustardId)},
                    t.{nameof(CustardCaliperLink.CaliperId)},
                    t.{nameof(CustardCaliperLink.MatchingPhone)}
                FROM {tempTable} t
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM {TableNames.CustardCaliperLinksName} c
                    WHERE c.{nameof(CustardCaliperLink.CustardId)} = t.{nameof(CustardCaliperLink.CustardId)}
                      AND c.{nameof(CustardCaliperLink.CaliperId)} = t.{nameof(CustardCaliperLink.CaliperId)}
                );
            """, ct);

            await _context.Database.ExecuteSqlRawAsync($"DELETE FROM {tempTable};", ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Inserted={Inserted}, Skipped={Skipped}",
                nameof(CustardCaliperLink),
                entities.Count,
                uniqueEntities.Count,
                stagedCount,
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
            _logger.LogError(ex, "{Entity} upsert failed", nameof(CustardCaliperLink));
            return Result.Failure<List<CustardCaliperLink>>(ex.Message);
        }

        void InsertBatch(List<CustardCaliperLink> batch)
        {
            var sql = new StringBuilder();
            sql.Append($"INSERT INTO {tempTable} VALUES ");

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];
                sql.Append($"({e.CustardId}, {e.CaliperId}, {e.MatchingPhone})");

                if (i < batch.Count - 1)
                    sql.Append(", ");
            }

            sql.Append(';');
            _context.Database.ExecuteSqlRaw(sql.ToString());
        }
    }

}
