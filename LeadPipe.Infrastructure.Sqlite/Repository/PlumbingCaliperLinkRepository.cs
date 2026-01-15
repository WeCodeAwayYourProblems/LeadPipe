using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class PlumbingCaliperLinkRepository
    (
        PlumbingContext context,
        ILogger<PlumbingCaliperLinkRepository> logger
    ) : PlumbingContextRepository<PlumbingCaliperLink, PlumbingCaliperLinkRepository>(context, logger), IPlumbingCaliperLinkRepository
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
            return Result.Success(new List<PlumbingCaliperLink>());

        AssertNotString<PlumbingCaliperLink>(nameof(PlumbingCaliperLink.PlumbingId));
        AssertNotString<PlumbingCaliperLink>(nameof(PlumbingCaliperLink.CaliperId));

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
                PRIMARY KEY ({nameof(PlumbingCaliperLink.PlumbingId)}, {nameof(PlumbingCaliperLink.CaliperId)})
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

            // Update existing rows
            await _context.Database.ExecuteSqlRawAsync($"""
            UPDATE {TableNames.PlumbingCaliperLinksName}
            SET {nameof(PlumbingCaliperLink.MatchingPhone)} = (
                SELECT 1
                FROM {tempTable} t
                WHERE t.{nameof(PlumbingCaliperLink.PlumbingId)} = {TableNames.PlumbingCaliperLinksName}.{nameof(PlumbingCaliperLink.PlumbingId)}
                  AND t.{nameof(PlumbingCaliperLink.CaliperId)} = {TableNames.PlumbingCaliperLinksName}.{nameof(PlumbingCaliperLink.CaliperId)}
            )
            WHERE EXISTS (
                SELECT 1
                FROM {tempTable} t
                WHERE t.{nameof(PlumbingCaliperLink.PlumbingId)} = {TableNames.PlumbingCaliperLinksName}.{nameof(PlumbingCaliperLink.PlumbingId)}
                  AND t.{nameof(PlumbingCaliperLink.CaliperId)} = {TableNames.PlumbingCaliperLinksName}.{nameof(PlumbingCaliperLink.CaliperId)}
            );
        """, cancellationToken: ct);

            // Insert missing rows
            int inserted = await _context.Database.ExecuteSqlRawAsync($"""
            INSERT INTO {TableNames.PlumbingCaliperLinksName} ({nameof(PlumbingCaliperLink.PlumbingId)}, {nameof(PlumbingCaliperLink.CaliperId)})
            SELECT t.{nameof(PlumbingCaliperLink.PlumbingId)}, t.{nameof(PlumbingCaliperLink.CaliperId)}
            FROM {tempTable} t
            WHERE NOT EXISTS (
                SELECT 1
                FROM {TableNames.PlumbingCaliperLinksName} p
                WHERE p.{nameof(PlumbingCaliperLink.PlumbingId)} = t.{nameof(PlumbingCaliperLink.PlumbingId)}
                  AND p.{nameof(PlumbingCaliperLink.CaliperId)} = t.{nameof(PlumbingCaliperLink.CaliperId)}
            );
        """, cancellationToken: ct);

            await _context.Database.ExecuteSqlRawAsync($"DELETE FROM {tempTable};", cancellationToken: ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Inserted={Inserted}, Skipped={Skipped}",
                nameof(PlumbingCaliperLink),
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
            _logger.LogError(ex, "{Entity} upsert failed", nameof(PlumbingCaliperLink));
            return Result.Failure<List<PlumbingCaliperLink>>(ex.Message);
        }

        void InsertBatch(List<PlumbingCaliperLink> batch)
        {
            var sql = new StringBuilder();
            sql.Append($"INSERT INTO {tempTable} VALUES ");

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];
                sql.Append($"({e.PlumbingId}, {e.CaliperId}, {e.MatchingPhone})");

                if (i < batch.Count - 1)
                    sql.Append(", ");
            }

            sql.Append(';');
            _context.Database.ExecuteSqlRaw(sql.ToString());
        }
    }

}
