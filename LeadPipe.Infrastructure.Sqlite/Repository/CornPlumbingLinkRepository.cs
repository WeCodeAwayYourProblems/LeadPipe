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

        // Deduplicate by (CornId, PlumbingId)
        List<CornPlumbingLink> uniqueEntities =
        [
            .. entities
                .GroupBy(e => (e.CornId, e.PlumbingId))
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
                    PRIMARY KEY ({nameof(CornPlumbingLink.CornId)}, {nameof(CornPlumbingLink.PlumbingId)})
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

            // ---- UPDATE existing ----
            await _context.Database.ExecuteSqlRawAsync($"""
                UPDATE {TableNames.CornPlumbingLinksName}
                SET {nameof(CornPlumbingLink.MatchingPhone)} = (
                    SELECT t.{nameof(CornPlumbingLink.MatchingPhone)}
                    FROM {tempTable} t
                    WHERE t.{nameof(CornPlumbingLink.CornId)} = {TableNames.CornPlumbingLinksName}.{nameof(CornPlumbingLink.CornId)}
                      AND t.{nameof(CornPlumbingLink.PlumbingId)} = {TableNames.CornPlumbingLinksName}.{nameof(CornPlumbingLink.PlumbingId)}
                )
                WHERE EXISTS (
                    SELECT 1
                    FROM {tempTable} t
                    WHERE t.{nameof(CornPlumbingLink.CornId)} = {TableNames.CornPlumbingLinksName}.{nameof(CornPlumbingLink.CornId)}
                      AND t.{nameof(CornPlumbingLink.PlumbingId)} = {TableNames.CornPlumbingLinksName}.{nameof(CornPlumbingLink.PlumbingId)}
                );
            """, ct);

            // ---- INSERT missing ----
            int inserted = await _context.Database.ExecuteSqlRawAsync($"""
                INSERT INTO {TableNames.CornPlumbingLinksName}
                    ({nameof(CornPlumbingLink.CornId)}, {nameof(CornPlumbingLink.PlumbingId)}, {nameof(CornPlumbingLink.MatchingPhone)})
                SELECT
                    t.{nameof(CornPlumbingLink.CornId)},
                    t.{nameof(CornPlumbingLink.PlumbingId)},
                    t.{nameof(CornPlumbingLink.MatchingPhone)}
                FROM {tempTable} t
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM {TableNames.CornPlumbingLinksName} c
                    WHERE c.{nameof(CornPlumbingLink.CornId)} = t.{nameof(CornPlumbingLink.CornId)}
                      AND c.{nameof(CornPlumbingLink.PlumbingId)} = t.{nameof(CornPlumbingLink.PlumbingId)}
                );
            """, ct);

            await _context.Database.ExecuteSqlRawAsync($"DELETE FROM {tempTable};", ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Inserted={Inserted}, Skipped={Skipped}",
                nameof(CornPlumbingLink),
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
            _logger.LogError(ex, "{Entity} upsert failed", nameof(CornPlumbingLink));
            return Result.Failure<List<CornPlumbingLink>>(ex.Message);
        }

        void InsertBatch(List<CornPlumbingLink> batch)
        {
            var sql = new StringBuilder();
            sql.Append($"INSERT INTO {tempTable} VALUES ");

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];
                sql.Append($"({e.CornId}, {e.PlumbingId}, {e.MatchingPhone})");

                if (i < batch.Count - 1)
                    sql.Append(", ");
            }

            sql.Append(';');
            _context.Database.ExecuteSqlRaw(sql.ToString());
        }
    }
}
