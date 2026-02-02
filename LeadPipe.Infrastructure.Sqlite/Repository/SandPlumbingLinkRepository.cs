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

        // Deduplicate in-memory
        List<SandPlumbingLink> uniqueEntities =
        [
            .. entities
                .GroupBy(e => (e.SandId, e.PlumbingId))
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
                    PRIMARY KEY ({nameof(SandPlumbingLink.SandId)}, {nameof(SandPlumbingLink.PlumbingId)})
                ) WITHOUT ROWID;
            """, cancellationToken: ct);

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
                       nameof(SandPlumbingLink),
                       batchSize,
                       ex.Message);

                    if (batchSize == minBatchSize)
                    {
                        var row = batch[0];
                        _logger.LogError(
                            "{Entity} row insert failed: SandId={SandId}, PlumbingId={PlumbingId}, MatchingPhone={MatchingPhone}",
                            nameof(SandPlumbingLink),
                            row.SandId,
                            row.PlumbingId,
                            row.MatchingPhone);

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
            int updated = await _context.Database.ExecuteSqlRawAsync($"""
                UPDATE {TableNames.SandPlumbingLinksName}
                SET MatchingPhone = (
                    SELECT t.{nameof(SandPlumbingLink.MatchingPhone)}
                    FROM {tempTable} t
                    WHERE t.{nameof(SandPlumbingLink.SandId)} = {TableNames.SandPlumbingLinksName}.{nameof(SandPlumbingLink.SandId)}
                      AND t.{nameof(SandPlumbingLink.PlumbingId)} = {TableNames.SandPlumbingLinksName}.{nameof(SandPlumbingLink.PlumbingId)}
                )
                WHERE EXISTS (
                    SELECT 1
                    FROM {tempTable} t
                    WHERE t.{nameof(SandPlumbingLink.SandId)} = {TableNames.SandPlumbingLinksName}.{nameof(SandPlumbingLink.SandId)}
                      AND t.{nameof(SandPlumbingLink.PlumbingId)} = {TableNames.SandPlumbingLinksName}.{nameof(SandPlumbingLink.PlumbingId)}
                );
            """, cancellationToken: ct);

            // ---- Phase 2: INSERT missing rows ----
            int inserted = await _context.Database.ExecuteSqlRawAsync($"""
                INSERT INTO {TableNames.SandPlumbingLinksName} ({nameof(SandPlumbingLink.SandId)}, {nameof(SandPlumbingLink.PlumbingId)}, {nameof(SandPlumbingLink.MatchingPhone)})
                SELECT t.{nameof(SandPlumbingLink.SandId)}, t.{nameof(SandPlumbingLink.PlumbingId)}, t.{nameof(SandPlumbingLink.MatchingPhone)}
                FROM {tempTable} t
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM {TableNames.SandPlumbingLinksName} s
                    WHERE s.{nameof(SandPlumbingLink.SandId)} = t.{nameof(SandPlumbingLink.SandId)}
                      AND s.{nameof(SandPlumbingLink.PlumbingId)} = t.{nameof(SandPlumbingLink.PlumbingId)}
                );
            """, cancellationToken: ct);

            await _context.Database.ExecuteSqlRawAsync($"DELETE FROM {tempTable};", cancellationToken: ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Updated={Updated}, Inserted={Inserted}, Skipped={Skipped}",
                nameof(SandPlumbingLink),
                entities.Count,
                uniqueEntities.Count,
                stagedCount,
                updated,
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
                nameof(SandPlumbingLink),
                ex.Message);
            return Result.Failure<List<SandPlumbingLink>>(ex.Message);
        }

        void InsertBatch(List<SandPlumbingLink> batch)
        {
            var sql = new StringBuilder();
            sql.Append($"INSERT INTO {tempTable} VALUES ");

            AssertNotString<SandPlumbingLink>(nameof(SandPlumbingLink.SandId));
            AssertNotString<SandPlumbingLink>(nameof(SandPlumbingLink.PlumbingId));
            AssertNotString<SandPlumbingLink>(nameof(SandPlumbingLink.MatchingPhone));

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];
                sql.Append($"({e.SandId}, {e.PlumbingId}, {e.MatchingPhone})");

                if (i < batch.Count - 1)
                    sql.Append(", ");
            }

            sql.Append(';');
            _context.Database.ExecuteSqlRaw(sql.ToString());
        }
    }

}
