using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class CaliperRepository
    (
        PlumbingContext context,
        ILogger<CaliperRepository> logger
    ) : PlumbingContextRepository<CaliperEntity, CaliperRepository>(context, logger), IRepository<CaliperEntity>
{
    protected override IQueryable<CaliperEntity> WithIncludes(IQueryable<CaliperEntity> q)
    {
        return q
            .Include(c => c.CustardCaliperLinks)
            .Include(c => c.SandCaliperLinks)
            .Include(c => c.PlumbingCaliperLinks)
            .Include(c => c.CornCaliperLinks);
    }

    public override async Task<Result<List<CaliperEntity>>> UpsertRangeAsync(
        List<CaliperEntity> entities,
        CancellationToken ct = default)
    {
        if (entities.Count == 0)
            return Result.Success(new List<CaliperEntity>());

        AssertNotString<CaliperEntity>(nameof(CaliperEntity.PhoneNumber));
        AssertNotString<CaliperEntity>(nameof(CaliperEntity.Date));
        AssertNotString<CaliperEntity>(nameof(CaliperEntity.UnixDate));
        AssertNotString<CaliperEntity>(nameof(CaliperEntity.Duration));

        int batchSize = 200;
        const int minBatchSize = 1;
        int stagedCount = 0;
        int skipped = 0;
        const string tempTable = "temp_caliper";

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            // Use TEXT for Date to match SQLite's date string preference, or INTEGER if storing ticks
            await _context.Database.ExecuteSqlRawAsync($"""
                CREATE TEMP TABLE IF NOT EXISTS {tempTable} (
                    {nameof(CaliperEntity.Id)} INTEGER PRIMARY KEY,
                    {nameof(CaliperEntity.PhoneNumber)} INTEGER NOT NULL,
                    {nameof(CaliperEntity.Date)} TEXT NOT NULL,
                    {nameof(CaliperEntity.UnixDate)} INTEGER NOT NULL,
                    {nameof(CaliperEntity.Note)} TEXT,
                    {nameof(CaliperEntity.Source)} TEXT,
                    {nameof(CaliperEntity.Location)} TEXT,
                    {nameof(CaliperEntity.Duration)} INTEGER,
                    {nameof(CaliperEntity.Billable)} INTEGER
                ) WITHOUT ROWID;
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

                    if (batchSize < 200)
                        batchSize = Math.Min(batchSize * 2, 200);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "{Entity} batch insert failed (size={BatchSize}). Reducing batch size.",
                        nameof(CaliperEntity),
                        batchSize);

                    if (batchSize == minBatchSize)
                    {
                        skipped++;
                        index++;
                        batchSize = 100;
                    }
                    else
                    {
                        batchSize = Math.Max(minBatchSize, batchSize / 2);
                    }
                }
            }

            // Target index is the Primary Key (Id)
            string updateSql = $"""
                UPDATE {TableNames.CaliperEntitiesName}
                SET
                    {nameof(CaliperEntity.PhoneNumber)} = (
                        SELECT temp.{nameof(CaliperEntity.PhoneNumber)}
                        FROM {tempTable} temp
                        WHERE temp.{nameof(CaliperEntity.Id)} = {TableNames.CaliperEntitiesName}.{nameof(CaliperEntity.Id)}
                    ),
                    {nameof(CaliperEntity.Date)} = (
                        SELECT temp.{nameof(CaliperEntity.Date)}
                        FROM {tempTable} temp
                        WHERE temp.{nameof(CaliperEntity.Id)} = {TableNames.CaliperEntitiesName}.{nameof(CaliperEntity.Id)}
                    ),
                    {nameof(CaliperEntity.UnixDate)} = (
                        SELECT temp.{nameof(CaliperEntity.UnixDate)}
                        FROM {tempTable} temp
                        WHERE temp.{nameof(CaliperEntity.Id)} = {TableNames.CaliperEntitiesName}.{nameof(CaliperEntity.Id)}
                    ),
                    {nameof(CaliperEntity.Note)} = (
                        SELECT temp.{nameof(CaliperEntity.Note)}
                        FROM {tempTable} temp
                        WHERE temp.{nameof(CaliperEntity.Id)} = {TableNames.CaliperEntitiesName}.{nameof(CaliperEntity.Id)}
                    ),
                    {nameof(CaliperEntity.Source)} = (
                        SELECT temp.{nameof(CaliperEntity.Source)}
                        FROM {tempTable} temp
                        WHERE temp.{nameof(CaliperEntity.Id)} = {TableNames.CaliperEntitiesName}.{nameof(CaliperEntity.Id)}
                    ),
                    {nameof(CaliperEntity.Location)} = (
                        SELECT temp.{nameof(CaliperEntity.Location)}
                        FROM {tempTable} temp
                        WHERE temp.{nameof(CaliperEntity.Id)} = {TableNames.CaliperEntitiesName}.{nameof(CaliperEntity.Id)}
                    ),
                    {nameof(CaliperEntity.Duration)} = (
                        SELECT temp.{nameof(CaliperEntity.Duration)}
                        FROM {tempTable} temp
                        WHERE temp.{nameof(CaliperEntity.Id)} = {TableNames.CaliperEntitiesName}.{nameof(CaliperEntity.Id)}
                    ),
                    {nameof(CaliperEntity.Billable)} = (
                        SELECT temp.{nameof(CaliperEntity.Billable)}
                        FROM {tempTable} temp
                        WHERE temp.{nameof(CaliperEntity.Id)} = {TableNames.CaliperEntitiesName}.{nameof(CaliperEntity.Id)}
                    )
                WHERE EXISTS (
                    SELECT 1
                    FROM {tempTable} temp
                    WHERE temp.{nameof(CaliperEntity.Id)} = {TableNames.CaliperEntitiesName}.{nameof(CaliperEntity.Id)}
                );
            """;
            int updatedCount = await _context.Database.ExecuteSqlRawAsync(updateSql, ct);

            // Insert new rows
            string insertSql = $"""
                INSERT INTO {TableNames.CaliperEntitiesName} (
                    {nameof(CaliperEntity.Id)}, 
                    {nameof(CaliperEntity.PhoneNumber)}, 
                    {nameof(CaliperEntity.Date)}, 
                    {nameof(CaliperEntity.UnixDate)}, 
                    {nameof(CaliperEntity.Note)}, 
                    {nameof(CaliperEntity.Source)}, 
                    {nameof(CaliperEntity.Location)}, 
                    {nameof(CaliperEntity.Duration)}, 
                    {nameof(CaliperEntity.Billable)}
                )
                SELECT
                    {nameof(CaliperEntity.Id)}, 
                    {nameof(CaliperEntity.PhoneNumber)}, 
                    {nameof(CaliperEntity.Date)}, 
                    {nameof(CaliperEntity.UnixDate)}, 
                    {nameof(CaliperEntity.Note)}, 
                    {nameof(CaliperEntity.Source)}, 
                    {nameof(CaliperEntity.Location)}, 
                    {nameof(CaliperEntity.Duration)}, 
                    {nameof(CaliperEntity.Billable)}
                FROM {tempTable} temp
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM {TableNames.CaliperEntitiesName} t
                    WHERE t.{nameof(CaliperEntity.Id)} = temp.{nameof(CaliperEntity.Id)}
                );
            """;
            int insertedCount = await _context.Database.ExecuteSqlRawAsync(insertSql, ct);

            await _context.Database.ExecuteSqlRawAsync(
                $"DELETE FROM {tempTable};", ct);

            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Staged={Staged}, Updated={Updated}, Inserted={Inserted}, Skipped={Skipped}",
                nameof(CaliperEntity),
                entities.Count,
                stagedCount,
                updatedCount,
                insertedCount,
                skipped);

            return Result.Success(entities);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Entity} upsert failed.", 
                nameof(CaliperEntity));
            return Result.Failure<List<CaliperEntity>>(ex.ToString());
        }

        void InsertBatch(List<CaliperEntity> batch)
        {
            var values = new List<object>();
            var rows = new List<string>();
            const int colsPerRow = 9;

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];
                int offset = i * colsPerRow;

                // Build placeholder string: ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})
                rows.Add($"({{{offset}}}, {{{offset + 1}}}, {{{offset + 2}}}, {{{offset + 3}}}, {{{offset + 4}}}, {{{offset + 5}}}, {{{offset + 6}}}, {{{offset + 7}}}, {{{offset + 8}}})");

                values.Add(e.Id);
                values.Add(e.PhoneNumber.Number); // Extract long from PhoneNumber object
                values.Add(e.Date.ToString("yyyy-MM-dd HH:mm:ss")); // ISO String for SQLite
                values.Add(e.UnixDate);
                values.Add(e.Note ?? (object)DBNull.Value);
                values.Add(e.Source ?? (object)DBNull.Value);
                values.Add(e.Location ?? (object)DBNull.Value);
                values.Add(e.Duration);
                values.Add(e.Billable ? 1 : 0);
            }

            string sql = $"INSERT INTO {tempTable} VALUES {string.Join(",", rows)};";
            _context.Database.ExecuteSqlRaw(sql, [.. values]);
        }
    }

}
