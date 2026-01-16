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

        // Deduplicate by (CustardId, PlumbingId)
        List<CustardPlumbingLink> uniqueEntities =
        [
            .. entities
                .GroupBy(e => (e.CustardId, e.PlumbingId))
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
                    PRIMARY KEY ({nameof(CustardPlumbingLink.CustardId)}, {nameof(CustardPlumbingLink.PlumbingId)})
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
                        nameof(CustardPlumbingLink),
                        batchSize);

                    if (batchSize == minBatchSize)
                    {
                        var row = batch[0];
                        _logger.LogError(
                            "{Entity} row insert failed: PlumbingId={PlumbingId}, PlumbingId={PlumbingId}",
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

            // ---- UPDATE existing ----
            await _context.Database.ExecuteSqlRawAsync($"""
                UPDATE {TableNames.CustardPlumbingLinksName}
                SET {nameof(CustardPlumbingLink.MatchingPhone)} = (
                    SELECT t.{nameof(CustardPlumbingLink.MatchingPhone)}
                    FROM {tempTable} t
                    WHERE t.{nameof(CustardPlumbingLink.CustardId)} = {TableNames.CustardPlumbingLinksName}.{nameof(CustardPlumbingLink.CustardId)}
                      AND t.{nameof(CustardPlumbingLink.PlumbingId)} = {TableNames.CustardPlumbingLinksName}.{nameof(CustardPlumbingLink.PlumbingId)}
                )
                WHERE EXISTS (
                    SELECT 1
                    FROM {tempTable} t
                    WHERE t.{nameof(CustardPlumbingLink.CustardId)} = {TableNames.CustardPlumbingLinksName}.{nameof(CustardPlumbingLink.CustardId)}
                      AND t.{nameof(CustardPlumbingLink.PlumbingId)} = {TableNames.CustardPlumbingLinksName}.{nameof(CustardPlumbingLink.PlumbingId)}
                );
            """, ct);

            // ---- INSERT missing ----
            int inserted = await _context.Database.ExecuteSqlRawAsync($"""
                INSERT INTO {TableNames.CustardPlumbingLinksName}
                    ({nameof(CustardPlumbingLink.CustardId)}, {nameof(CustardPlumbingLink.PlumbingId)}, {nameof(CustardPlumbingLink.MatchingPhone)})
                SELECT
                    t.{nameof(CustardPlumbingLink.CustardId)},
                    t.{nameof(CustardPlumbingLink.PlumbingId)},
                    t.{nameof(CustardPlumbingLink.MatchingPhone)}
                FROM {tempTable} t
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM {TableNames.CustardPlumbingLinksName} c
                    WHERE c.{nameof(CustardPlumbingLink.CustardId)} = t.{nameof(CustardPlumbingLink.CustardId)}
                      AND c.{nameof(CustardPlumbingLink.PlumbingId)} = t.{nameof(CustardPlumbingLink.PlumbingId)}
                );
            """, ct);

            await _context.Database.ExecuteSqlRawAsync($"DELETE FROM {tempTable};", ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Inserted={Inserted}, Skipped={Skipped}",
                nameof(CustardPlumbingLink),
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
            _logger.LogError(ex, "{Entity} upsert failed", nameof(CustardPlumbingLink));
            return Result.Failure<List<CustardPlumbingLink>>(ex.Message);
        }

        void InsertBatch(List<CustardPlumbingLink> batch)
        {
            var sql = new StringBuilder();
            sql.Append($"INSERT INTO {tempTable} VALUES ");

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];
                sql.Append($"({e.CustardId}, {e.PlumbingId}, {e.MatchingPhone})");

                if (i < batch.Count - 1)
                    sql.Append(", ");
            }

            sql.Append(';');
            _context.Database.ExecuteSqlRaw(sql.ToString());
        }
    }

}
