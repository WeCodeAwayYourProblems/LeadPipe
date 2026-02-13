using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class CustardCornLinkRepository
    (
        PlumbingContext context,
        ILogger<CustardCornLinkRepository> logger
    ) : PlumbingContextRepository<CustardCornLink, CustardCornLinkRepository>(context, logger), IRepository<CustardCornLink>
{
    protected override IQueryable<CustardCornLink> WithIncludes(IQueryable<CustardCornLink> q)
    {
        return q
            .Include(q => q.Custard)
            .Include(q => q.Corn);
    }

    public override async Task<Result<List<CustardCornLink>>> UpsertRangeAsync(List<CustardCornLink> entities, CancellationToken ct = default)
    {
        if (entities.Count == 0)
            return Result.Success(new List<CustardCornLink>());

        AssertNotString<CustardCornLink>(nameof(CustardCornLink.CustardId));
        AssertNotString<CustardCornLink>(nameof(CustardCornLink.CornId));
        AssertNotString<CustardCornLink>(nameof(CustardCornLink.MatchingPhone));
        AssertNotString<CustardCornLink>(nameof(CustardCornLink.UnixMatchDate));

        // Deduplicate by (CustardId, CornId)
        List<CustardCornLink> uniqueEntities =
        [
            .. entities
                .Where(e => e.MatchingPhone != 0)
                .GroupBy(e => (e.CustardId, e.CornId))
                .Select(g => g.Last())
        ];

        int batchSize = 200;
        const int minBatchSize = 1;
        int stagedCount = 0;
        int skipped = 0;
        const string tempTable = "temp_custard_corn_links";

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            // Temp table
            await _context.Database.ExecuteSqlRawAsync($"""
                CREATE TEMP TABLE IF NOT EXISTS {tempTable} (
                    {nameof(CustardCornLink.CustardId)} INTEGER NOT NULL,
                    {nameof(CustardCornLink.CornId)} INTEGER NOT NULL,
                    {nameof(CustardCornLink.MatchingPhone)} INTEGER NOT NULL CHECK({nameof(CustardCornLink.MatchingPhone)} <> 0),
                    {nameof(CustardCornLink.UnixMatchDate)} INTEGER NOT NULL,
                    PRIMARY KEY ({nameof(CustardCornLink.CustardId)}, {nameof(CustardCornLink.CornId)})
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
                    _logger.LogError(
                        ex,
                        "{Entity} batch insert failed (size={BatchSize}). Reducing batch size. Exception Message: {Message}",
                        nameof(CustardCornLink),
                        batchSize,
                        ex.Message);

                    if (batchSize == minBatchSize)
                    {
                        var row = batch[0];
                        _logger.LogError(
                            "{Entity} row insert failed: CustardId={CustardId}, CornId={CornId}",
                            nameof(CustardCornLink),
                            row.CustardId,
                            row.CornId);

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
                UPDATE {TableNames.CustardCornLinksName}
                SET {nameof(CustardCornLink.MatchingPhone)} = (
                    SELECT t.{nameof(CustardCornLink.MatchingPhone)}
                    FROM {tempTable} t
                    WHERE t.{nameof(CustardCornLink.CustardId)} = {TableNames.CustardCornLinksName}.{nameof(CustardCornLink.CustardId)}
                      AND t.{nameof(CustardCornLink.CornId)} = {TableNames.CustardCornLinksName}.{nameof(CustardCornLink.CornId)}
                ),
                    {nameof(CustardCornLink.UnixMatchDate)} = (
                    SELECT t.{nameof(CustardCornLink.UnixMatchDate)}
                    FROM {tempTable} t
                    WHERE t.{nameof(CustardCornLink.CustardId)} = {TableNames.CustardCornLinksName}.{nameof(CustardCornLink.CustardId)}
                        AND t.{nameof(CustardCornLink.CornId)} = {TableNames.CustardCornLinksName}.{nameof(CustardCornLink.CornId)}
                    LIMIT 1
                )
                WHERE EXISTS (
                    SELECT 1
                    FROM {tempTable} t
                    WHERE t.{nameof(CustardCornLink.CustardId)} = {TableNames.CustardCornLinksName}.{nameof(CustardCornLink.CustardId)}
                      AND t.{nameof(CustardCornLink.CornId)} = {TableNames.CustardCornLinksName}.{nameof(CustardCornLink.CornId)}
                );
            """, ct);

            // ---- INSERT missing ----
            int inserted = await _context.Database.ExecuteSqlRawAsync($"""
                INSERT INTO {TableNames.CustardCornLinksName}
                    ({nameof(CustardCornLink.CustardId)}, {nameof(CustardCornLink.CornId)}, {nameof(CustardCornLink.MatchingPhone)}, {nameof(CustardCornLink.UnixMatchDate)})
                SELECT
                    t.{nameof(CustardCornLink.CustardId)},
                    t.{nameof(CustardCornLink.CornId)},
                    t.{nameof(CustardCornLink.MatchingPhone)},
                    t.{nameof(CustardCornLink.UnixMatchDate)}
                FROM {tempTable} t
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM {TableNames.CustardCornLinksName} c
                    WHERE c.{nameof(CustardCornLink.CustardId)} = t.{nameof(CustardCornLink.CustardId)}
                      AND c.{nameof(CustardCornLink.CornId)} = t.{nameof(CustardCornLink.CornId)}
                )
                    AND EXISTS (
                        SELECT 1 
                        FROM {TableNames.CustardEntitiesName} p 
                        WHERE p.{nameof(CustardEntity.Id)} = t.{nameof(CustardCornLink.CustardId)}
                    )
                    AND EXISTS (
                        SELECT 1 
                        FROM {TableNames.CornEntitiesName} p 
                        WHERE p.{nameof(CornEntity.Id)} = t.{nameof(CustardCornLink.CornId)}
                    );
            """, ct);

            await _context.Database.ExecuteSqlRawAsync($"DELETE FROM {tempTable};", ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Inserted={Inserted}, Skipped={Skipped}",
                nameof(CustardCornLink),
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
            _logger.LogError(ex, "{Entity} upsert failed. Exception Message: {Message}",
                nameof(CustardCornLink),
                ex.Message);
            return Result.Failure<List<CustardCornLink>>(ex.Message);
        }

        void InsertBatch(List<CustardCornLink> batch)
        {
            var sql = new StringBuilder();
            sql.Append($"INSERT INTO {tempTable} VALUES ");

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];
                sql.Append($"({e.CustardId}, {e.CornId}, {e.MatchingPhone}, {e.UnixMatchDate})");

                if (i < batch.Count - 1)
                    sql.Append(", ");
            }

            sql.Append(';');
            _context.Database.ExecuteSqlRaw(sql.ToString());
        }
    }

}
