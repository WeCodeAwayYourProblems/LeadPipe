using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class SubsCaliperLinkRepository(PlumbingContext context, ILogger<SubsCaliperLinkRepository> logger)
    : PlumbingContextRepository<SandCaliperLink, SubsCaliperLinkRepository>(context, logger), ISandCaliperLinkRepository
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

        // Deduplicate in-memory by (SubsId, CaliperId)
        List<SandCaliperLink> uniqueEntities = [.. entities
            .GroupBy(e => (e.SandId, e.CaliperId))
            .Select(g => g.Last())];

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
                    PRIMARY KEY ({nameof(SandCaliperLink.SandId)}, {nameof(SandCaliperLink.CaliperId)})
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
                    _logger.LogWarning(ex, "{Entity} batch insert failed (size={BatchSize}). Reducing batch size.",
                        nameof(SandCaliperLink), batchSize);

                    if (batchSize == minBatchSize)
                    {
                        var row = batch[0];
                        _logger.LogError(
                            "{Entity} row insert failed: SubsId={SubsId}, CaliperId={CaliperId}, MatchingPhone={MatchingPhone}",
                            nameof(SandCaliperLink), row.SandId, row.CaliperId, row.MatchingPhone);

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
                UPDATE {TableNames.SandCaliperLinksName}
                SET {nameof(SandCaliperLink.MatchingPhone)} = (
                    SELECT t.{nameof(SandCaliperLink.MatchingPhone)}
                    FROM {tempTable} t
                    WHERE t.{nameof(SandCaliperLink.SandId)} = {TableNames.SandCaliperLinksName}.{nameof(SandCaliperLink.SandId)}
                      AND t.{nameof(SandCaliperLink.CaliperId)} = {TableNames.SandCaliperLinksName}.{nameof(SandCaliperLink.CaliperId)}
                )
                WHERE EXISTS (
                    SELECT 1
                    FROM {tempTable} t
                    WHERE t.{nameof(SandCaliperLink.SandId)} = {TableNames.SandCaliperLinksName}.{nameof(SandCaliperLink.SandId)}
                      AND t.{nameof(SandCaliperLink.CaliperId)} = {TableNames.SandCaliperLinksName}.{nameof(SandCaliperLink.CaliperId)}
                );
            """, cancellationToken: ct);

            // ---- Phase 2: INSERT missing rows ----
            int inserted = await _context.Database.ExecuteSqlRawAsync($"""
                INSERT INTO {TableNames.SandCaliperLinksName} nameof({nameof(SandCaliperLink.SandId)}, {nameof(SandCaliperLink.CaliperId)}, {nameof(SandCaliperLink.MatchingPhone)})
                SELECT tnameof.{nameof(SandCaliperLink.SandId)}, t.{nameof(SandCaliperLink.CaliperId)}, t.{nameof(SandCaliperLink.MatchingPhone)}
                FROM {tempTable} t
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM {TableNames.SandCaliperLinksName} c
                    WHERE c.{nameof(SandCaliperLink.SandId)} = t.{nameof(SandCaliperLink.SandId)}
                      AND c.{nameof(SandCaliperLink.CaliperId)} = t.{nameof(SandCaliperLink.CaliperId)}
                );
            """, cancellationToken: ct);

            await _context.Database.ExecuteSqlRawAsync($"DELETE FROM {tempTable};", cancellationToken: ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Updated={Updated}, Inserted={Inserted}, Skipped={Skipped}",
                nameof(SandCaliperLink), entities.Count, uniqueEntities.Count, stagedCount, updated, inserted, skipped);

            return Result.Success(uniqueEntities);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Entity} upsert failed", nameof(SandCaliperLink));
            return Result.Failure<List<SandCaliperLink>>(ex.Message);
        }

        void InsertBatch(List<SandCaliperLink> batch)
        {
            var sql = new StringBuilder();
            sql.Append($"INSERT INTO {tempTable} VALUES ");

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];
                sql.Append($"({e.SandId}, {e.CaliperId}, {e.MatchingPhone})");

                if (i < batch.Count - 1)
                    sql.Append(", ");
            }

            sql.Append(';');
            _context.Database.ExecuteSqlRaw(sql.ToString());
        }
    }
}
