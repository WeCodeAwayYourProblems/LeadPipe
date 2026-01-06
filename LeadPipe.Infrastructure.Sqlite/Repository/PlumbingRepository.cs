using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public class PlumbingRepository(PlumbingContext context, ILogger<PlumbingRepository> logger)
    : PlumbingContextRepository<PlumbingEntity, PlumbingRepository>(context, logger), IPlumbingRepository
{
    public async Task<Result<List<PlumbingEntity>>> GetAllAsync(Source source)
    {
        try
        {
            List<PlumbingEntity> plumbing = await _set
                .Where(p => p.Source == source)
                .ToListAsync();
            return Result.Success(plumbing);
        }
        catch (Exception ex) { return Result.Failure<List<PlumbingEntity>>(ex.ToString()); }
    }
    public override async Task<Result<List<PlumbingEntity>>> AddRangeAsync(List<PlumbingEntity> entities)
    {
        if (entities == null || entities.Count == 0)
            return Result.Failure<List<PlumbingEntity>>("No entities provided.");

        try
        {
            // Sort entities
            List<PlumbingEntity> sortedEntities = [.. entities.OrderBy(e => e.Date)];

            // Deduplicate input entities
            HashSet<(long, Source)> seenKeys = [];
            List<PlumbingEntity> uniqueEntities = [];
            uniqueEntities.AddRange(
                from e in sortedEntities
                let key = (e.PhoneNumber, e.Source)
                where seenKeys.Add(key) // This will be true only for the first key, so no duplicates are added
                select e);

            // Extract numbers from unique set
            HashSet<long> phoneNumbers = [.. uniqueEntities.Select(e => e.PhoneNumber)];

            // Query based on unique phone numbers
            var existing = await _set
                .Where(p => phoneNumbers.Contains(p.PhoneNumber))
                .Select(p => new { p.PhoneNumber, p.Source })
                .ToListAsync();

            // Now finish the composite match in memory
            HashSet<(long PhoneNumber, Source Source)> existingSet = [.. existing.Select(x => (x.PhoneNumber, x.Source))];

            List<PlumbingEntity> toInsert = [.. uniqueEntities.Where(e => !existingSet.Contains((e.PhoneNumber, e.Source)))];

            if (toInsert.Count == 0)
                return Result.Success(new List<PlumbingEntity>());

            await _set.AddRangeAsync(toInsert);
            await _context.SaveChangesAsync();

            _logger.LogDebug(
                "Plumbing bulk insert: {Inserted}/{Total}",
                toInsert.Count,
                entities.Count);

            return Result.Success(toInsert);
        }
        catch (Exception ex) { return Result.Failure<List<PlumbingEntity>>($"Failed to save Plumbing entities: {ex}"); }
    }

    public override async Task<Result<List<PlumbingEntity>>> UpsertRangeAsync(List<PlumbingEntity> entities)
    {
        if (entities == null || entities.Count == 0)
            return Result.Success(new List<PlumbingEntity>());

        // Deduplicate in-memory by (PhoneNumber, Source)
        List<PlumbingEntity> uniqueEntities = [.. entities
            .GroupBy(e => (e.PhoneNumber, e.Source))
            .Select(g => g.Last())];

        int batchSize = 200;
        const int minBatchSize = 1;
        int skipped = 0;
        int stagedCount = 0;

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            // Temp table for staging
            await _context.Database.ExecuteSqlRawAsync("""
                CREATE TEMP TABLE IF NOT EXISTS temp_plumbings (
                    PhoneNumber INTEGER NOT NULL,
                    Date TEXT NOT NULL,
                    UnixDate INTEGER NOT NULL,
                    Contents TEXT,
                    Source TEXT NOT NULL,
                    MetaData TEXT NOT NULL,
                    PRIMARY KEY (PhoneNumber, Source)
                ) WITHOUT ROWID;
            """);

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

                    // Gradually scale back up after success
                    if (batchSize < 200)
                        batchSize = Math.Min(batchSize * 2, 200);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Batch insert failed (size={BatchSize}). Reducing batch size.", batchSize);

                    if (batchSize == minBatchSize)
                    {
                        var row = batch[0];
                        _logger.LogError(
                            "Row insert failed: Phone={Phone}, Date={Date}, Contents={Contents}, Source={Source}, MetaData={MetaData}",
                            row.PhoneNumber, row.Date, row.Contents, row.Source, row.MetaData);

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
            int updated = await _context.Database.ExecuteSqlRawAsync("""
                UPDATE PlumbingEntities
                SET
                    Date = (SELECT t.Date FROM temp_plumbings t WHERE t.PhoneNumber = PlumbingEntities.PhoneNumber AND t.Source = PlumbingEntities.Source),
                    UnixDate = (SELECT t.UnixDate FROM temp_plumbings t WHERE t.PhoneNumber = PlumbingEntities.PhoneNumber AND t.Source = PlumbingEntities.Source),
                    Contents = (SELECT t.Contents FROM temp_plumbings t WHERE t.PhoneNumber = PlumbingEntities.PhoneNumber AND t.Source = PlumbingEntities.Source),
                    MetaData = (SELECT t.MetaData FROM temp_plumbings t WHERE t.PhoneNumber = PlumbingEntities.PhoneNumber AND t.Source = PlumbingEntities.Source)
                WHERE EXISTS (
                    SELECT 1 FROM temp_plumbings t WHERE t.PhoneNumber = PlumbingEntities.PhoneNumber AND t.Source = PlumbingEntities.Source
                );
            """);

            // ---- Phase 2: INSERT missing rows ----
            int inserted = await _context.Database.ExecuteSqlRawAsync("""
                INSERT INTO PlumbingEntities
                    (PhoneNumber, Date, UnixDate, Contents, Source, MetaData)
                SELECT
                    t.PhoneNumber,
                    t.Date,
                    t.UnixDate,
                    t.Contents,
                    t.Source,
                    t.MetaData
                FROM temp_plumbings t
                WHERE NOT EXISTS (
                    SELECT 1 FROM PlumbingEntities c WHERE c.PhoneNumber = t.PhoneNumber AND c.Source = t.Source
                );
            """);

            await _context.Database.ExecuteSqlRawAsync("DELETE FROM temp_plumbings;");
            await transaction.CommitAsync();

            _logger.LogInformation(
                "PlumbingEntity upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Updated={Updated}, Inserted={Inserted}, Skipped={Skipped}",
                entities.Count, uniqueEntities.Count, stagedCount, updated, inserted, skipped);

            return Result.Success(uniqueEntities);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PlumbingEntity upsert failed");
            return Result.Failure<List<PlumbingEntity>>(ex.ToString());
        }

        void InsertBatch(List<PlumbingEntity> batch)
        {
            var sql = new StringBuilder();
            sql.Append("INSERT INTO temp_plumbings VALUES ");

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];
                sql.Append($"""
                    (
                        {e.PhoneNumber},
                        '{e.Date:yyyy-MM-dd HH:mm:ss}',
                        {e.UnixDate},
                        '{Clean(e.Contents)}',
                        '{e.Source}',
                        '{Clean(e.MetaData)}'
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

        // Remove embedded nulls
        value = value.Replace("\0", string.Empty);

        // Escape single quotes for raw SQL
        return value.Replace("'", "''");
    }
}
