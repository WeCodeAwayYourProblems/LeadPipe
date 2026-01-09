using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class CaliperRepository(PlumbingContext context, ILogger<CaliperRepository> logger)
    : PlumbingContextRepository<CaliperEntity, CaliperRepository>(context, logger), ICaliperRepository
{
    public override async Task<Result<List<CaliperEntity>>> UpsertRangeAsync(List<CaliperEntity> entities)
    {
        if (entities.Count == 0)
            return Result.Success(new List<CaliperEntity>());

        // Deduplicate
        List<CaliperEntity> uniqueEntities =
        [
            .. entities
            .GroupBy(e => (e.PhoneNumber, e.CaliperDate))
            .Select(g => g.Last())
        ];

        int batchSize = 200;
        const int minBatchSize = 1;

        try
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            // Temp table (connection-scoped)
            await _context.Database.ExecuteSqlRawAsync("""
            CREATE TEMP TABLE IF NOT EXISTS temp_calls (
                PhoneNumber INTEGER NOT NULL,
                CaliperDate TEXT NOT NULL,
                UnixCaliperDate INTEGER NOT NULL,
                Note TEXT,
                Source TEXT,
                Location TEXT,
                Duration INTEGER,
                Billable INTEGER,
                PRIMARY KEY (PhoneNumber, CaliperDate)
            ) WITHOUT ROWID;
        """);

            int index = 0;
            int stagedCount = 0;
            int skipped = 0;

            while (index < uniqueEntities.Count)
            {
                int take = Math.Min(batchSize, uniqueEntities.Count - index);
                var batch = uniqueEntities.GetRange(index, take);

                try
                {
                    InsertBatch(batch);
                    stagedCount += batch.Count;
                    index += take;

                    // Gradually scale back up after success
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
                        var row = batch[0];

                        _logger.LogError(
                            "{Entity} Row insert failed: Phone={Phone}, CaliperDate={CaliperDate}, Note={Note}, Source={Source}, Location={Location}",
                            nameof(CaliperEntity),
                            row.PhoneNumber,
                            row.CaliperDate,
                            row.Note,
                            row.Source,
                            row.Location);

                        index++;
                        skipped++;
                        batchSize = 100;
                    }
                    else
                    {
                        batchSize = Math.Max(minBatchSize, batchSize / 2);
                    }
                }
            }

            // ---- Phase 1: UPDATE existing rows ----
            int updated = await _context.Database.ExecuteSqlRawAsync("""
            UPDATE CaliperEntities
            SET
                UnixCaliperDate = (
                    SELECT t.UnixCaliperDate
                    FROM temp_calls t
                    WHERE t.PhoneNumber = CaliperEntities.PhoneNumber
                      AND t.CaliperDate = CaliperEntities.CaliperDate
                ),
                Note = (
                    SELECT t.Note
                    FROM temp_calls t
                    WHERE t.PhoneNumber = CaliperEntities.PhoneNumber
                      AND t.CaliperDate = CaliperEntities.CaliperDate
                ),
                Source = (
                    SELECT t.Source
                    FROM temp_calls t
                    WHERE t.PhoneNumber = CaliperEntities.PhoneNumber
                      AND t.CaliperDate = CaliperEntities.CaliperDate
                ),
                Location = (
                    SELECT t.Location
                    FROM temp_calls t
                    WHERE t.PhoneNumber = CaliperEntities.PhoneNumber
                      AND t.CaliperDate = CaliperEntities.CaliperDate
                ),
                Duration = (
                    SELECT t.Duration
                    FROM temp_calls t
                    WHERE t.PhoneNumber = CaliperEntities.PhoneNumber
                      AND t.CaliperDate = CaliperEntities.CaliperDate
                ),
                Billable = (
                    SELECT t.Billable
                    FROM temp_calls t
                    WHERE t.PhoneNumber = CaliperEntities.PhoneNumber
                      AND t.CaliperDate = CaliperEntities.CaliperDate
                )
            WHERE EXISTS (
                SELECT 1
                FROM temp_calls t
                WHERE t.PhoneNumber = CaliperEntities.PhoneNumber
                  AND t.CaliperDate = CaliperEntities.CaliperDate
            );
        """);

            // ---- Phase 2: INSERT missing rows ----
            int inserted = await _context.Database.ExecuteSqlRawAsync("""
            INSERT INTO CaliperEntities
                (PhoneNumber, CaliperDate, UnixCaliperDate, Note, Source, Location, Duration, Billable)
            SELECT
                t.PhoneNumber,
                t.CaliperDate,
                t.UnixCaliperDate,
                t.Note,
                t.Source,
                t.Location,
                t.Duration,
                t.Billable
            FROM temp_calls t
            WHERE NOT EXISTS (
                SELECT 1
                FROM CaliperEntities c
                WHERE c.PhoneNumber = t.PhoneNumber
                  AND c.CaliperDate = t.CaliperDate
            );
        """);

            await _context.Database.ExecuteSqlRawAsync("DELETE FROM temp_calls;");
            await transaction.CommitAsync();

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Updated={Updated}, Inserted={Inserted}, Skipped={Skipped}",
                nameof(CaliperEntity),
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
            _logger.LogError(ex, "CaliperEntity upsert failed");
            return Result.Failure<List<CaliperEntity>>(ex.ToString());
        }

        // ---- Local helper ----
        void InsertBatch(List<CaliperEntity> batch)
        {
            var sql = new StringBuilder();
            sql.Append("INSERT INTO temp_calls VALUES ");

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];

                sql.Append($"""
                (
                    {e.PhoneNumber},
                    '{e.CaliperDate:yyyy-MM-dd HH:mm:ss}',
                    {e.UnixDate},
                    '{Clean(e.Note)}',
                    '{Clean(e.Source)}',
                    '{Clean(e.Location)}',
                    {e.Duration},
                    {(e.Billable ? 1 : 0)}
                )
            """);

                if (i < batch.Count - 1)
                    sql.Append(", ");
            }

            sql.Append(';');
            _context.Database.ExecuteSqlRaw(sql.ToString());
        }
    }

    private static string Clean(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // SQLite cannot handle embedded nulls
        value = value.Replace("\0", string.Empty);

        // Escape single quotes for raw SQL
        return value.Replace("'", "''");
    }
}