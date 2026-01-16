using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

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

        // Deduplicate by (CornId, CaliperId)
        List<CornCaliperLink> uniqueEntities =
        [
            .. entities
                .GroupBy(e => (e.CornId, e.CaliperId))
                .Select(g => g.Last())
        ];

        int batchSize = 200;
        const int minBatchSize = 1;
        int stagedCount = 0;
        int skipped = 0;
        const string tempTable = "temp_corn_caliper_links";

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            // Temp table
            await _context.Database.ExecuteSqlRawAsync($"""
                CREATE TEMP TABLE IF NOT EXISTS {tempTable} (
                    {nameof(CornCaliperLink.CornId)} INTEGER NOT NULL,
                    {nameof(CornCaliperLink.CaliperId)} INTEGER NOT NULL,
                    {nameof(CornCaliperLink.MatchingPhone)} INTEGER NOT NULL,
                    PRIMARY KEY ({nameof(CornCaliperLink.CornId)}, {nameof(CornCaliperLink.CaliperId)})
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

            // ---- UPDATE existing ----
            await _context.Database.ExecuteSqlRawAsync($"""
                UPDATE {TableNames.CornCaliperLinksName}
                SET {nameof(CornCaliperLink.MatchingPhone)} = (
                    SELECT t.{nameof(CornCaliperLink.MatchingPhone)}
                    FROM {tempTable} t
                    WHERE t.{nameof(CornCaliperLink.CornId)} = {TableNames.CornCaliperLinksName}.{nameof(CornCaliperLink.CornId)}
                      AND t.{nameof(CornCaliperLink.CaliperId)} = {TableNames.CornCaliperLinksName}.{nameof(CornCaliperLink.CaliperId)}
                )
                WHERE EXISTS (
                    SELECT 1
                    FROM {tempTable} t
                    WHERE t.{nameof(CornCaliperLink.CornId)} = {TableNames.CornCaliperLinksName}.{nameof(CornCaliperLink.CornId)}
                      AND t.{nameof(CornCaliperLink.CaliperId)} = {TableNames.CornCaliperLinksName}.{nameof(CornCaliperLink.CaliperId)}
                );
            """, ct);

            // ---- INSERT missing ----
            int inserted = await _context.Database.ExecuteSqlRawAsync($"""
                INSERT INTO {TableNames.CornCaliperLinksName}
                    ({nameof(CornCaliperLink.CornId)}, {nameof(CornCaliperLink.CaliperId)}, {nameof(CornCaliperLink.MatchingPhone)})
                SELECT
                    t.{nameof(CornCaliperLink.CornId)},
                    t.{nameof(CornCaliperLink.CaliperId)},
                    t.{nameof(CornCaliperLink.MatchingPhone)}
                FROM {tempTable} t
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM {TableNames.CornCaliperLinksName} c
                    WHERE c.{nameof(CornCaliperLink.CornId)} = t.{nameof(CornCaliperLink.CornId)}
                      AND c.{nameof(CornCaliperLink.CaliperId)} = t.{nameof(CornCaliperLink.CaliperId)}
                );
            """, ct);

            await _context.Database.ExecuteSqlRawAsync($"DELETE FROM {tempTable};", ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Inserted={Inserted}, Skipped={Skipped}",
                nameof(CornCaliperLink),
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
            _logger.LogError(ex, "{Entity} upsert failed", nameof(CornCaliperLink));
            return Result.Failure<List<CornCaliperLink>>(ex.Message);
        }

        void InsertBatch(List<CornCaliperLink> batch)
        {
            var sql = new StringBuilder();
            sql.Append($"INSERT INTO {tempTable} VALUES ");

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];
                sql.Append($"({e.CornId}, {e.CaliperId}, {e.MatchingPhone})");

                if (i < batch.Count - 1)
                    sql.Append(", ");
            }

            sql.Append(';');
            _context.Database.ExecuteSqlRaw(sql.ToString());
        }
    }
}
