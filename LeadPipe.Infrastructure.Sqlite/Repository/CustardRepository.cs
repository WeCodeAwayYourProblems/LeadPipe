using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class CustardRepository
    (
        PlumbingContext context,
        ILogger<CustardRepository> logger
    ) : PlumbingContextRepository<CustardEntity, CustardRepository>(context, logger), IRepository<CustardEntity>
{
    protected override IQueryable<CustardEntity> WithIncludes(IQueryable<CustardEntity> q)
    {
        return q
            .Include(q => q.SandEntities)
            .Include(q => q.CustardCaliperLinks)
            .Include(q => q.CustardCornLinks)
            .Include(q => q.CustardPlumbingLinks);
    }

    public override async Task<Result<List<CustardEntity>>> UpsertRangeAsync(List<CustardEntity> entities, CancellationToken ct = default)
    {
        if (entities.Count == 0)
            return Result.Success(new List<CustardEntity>());

        AssertNotString<CustardEntity>(nameof(CustardEntity.Id));
        AssertNotString<CustardEntity>(nameof(CustardEntity.Active));
        AssertNotString<CustardEntity>(nameof(CustardEntity.PhoneNumber));
        AssertNotString<CustardEntity>(nameof(CustardEntity.PhoneNumber2));
        AssertNotString<CustardEntity>(nameof(CustardEntity.Date));
        AssertNotString<CustardEntity>(nameof(CustardEntity.UnixDate));
        AssertNotString<CustardEntity>(nameof(CustardEntity.CancelDate));
        AssertNotString<CustardEntity>(nameof(CustardEntity.UnixCancelDate));

        // Deduplicate in-memory by Id
        List<CustardEntity> uniqueEntities =
        [
            .. entities
                .GroupBy(e => e.Id)
                .Select(g => g.Last())
        ];

        int batchSize = 50;
        const int minBatchSize = 1;
        int stagedCount = 0;
        int skipped = 0;
        const string tempTable = "temp_custard_entities";

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            // Temp table; create it and clear it
            await _context.Database.ExecuteSqlRawAsync($"""
                CREATE TEMP TABLE IF NOT EXISTS {tempTable} (
                    {nameof(CustardEntity.Id)} INTEGER PRIMARY KEY,
                    {nameof(CustardEntity.Active)} INTEGER,
                    {nameof(CustardEntity.PhoneNumber)} INTEGER,
                    {nameof(CustardEntity.PhoneNumber2)} INTEGER,
                    {nameof(CustardEntity.Date)} TEXT,
                    {nameof(CustardEntity.UnixDate)} INTEGER,
                    {nameof(CustardEntity.CancelDate)} TEXT,
                    {nameof(CustardEntity.UnixCancelDate)} INTEGER
                ) WITHOUT ROWID;
                DELETE FROM {tempTable};
            """, ct);

            int index = 0;

            while (index < uniqueEntities.Count)
            {
                ct.ThrowIfCancellationRequested();

                int take = Math.Min(batchSize, uniqueEntities.Count - index);
                var batch = uniqueEntities.GetRange(index, take);

                try
                {
                    InsertBatch(batch);
                    stagedCount += batch.Count;
                    index += take;

                    if (batchSize < 50)
                        batchSize = Math.Min(batchSize * 2, 50);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "{Entity} batch insert failed (size={BatchSize}). Reducing batch size.",
                        nameof(CustardEntity),
                        batchSize);

                    if (batchSize == minBatchSize)
                    {
                        var row = batch[0];
                        _logger.LogError(
                            "{Entity} row insert failed: Id={Id}",
                            nameof(CustardEntity),
                            row.Id);

                        index++;
                        batchSize = 25;
                        skipped++;
                    }
                    else
                    {
                        batchSize = Math.Max(minBatchSize, batchSize / 2);
                    }
                }
            }

            // Update existing rows
            string updateSql = $"""
                UPDATE {TableNames.CustardEntitiesName} 
                SET 
                    {nameof(CustardEntity.Active)} = temp.{nameof(CustardEntity.Active)},
                    {nameof(CustardEntity.PhoneNumber)} = temp.{nameof(CustardEntity.PhoneNumber)},
                    {nameof(CustardEntity.PhoneNumber2)} = temp.{nameof(CustardEntity.PhoneNumber2)},
                    {nameof(CustardEntity.Date)} = temp.{nameof(CustardEntity.Date)},
                    {nameof(CustardEntity.UnixDate)} = temp.{nameof(CustardEntity.UnixDate)},
                    {nameof(CustardEntity.CancelDate)} = temp.{nameof(CustardEntity.CancelDate)},
                    {nameof(CustardEntity.UnixCancelDate)} = temp.{nameof(CustardEntity.UnixCancelDate)}
                FROM {tempTable} temp
                WHERE temp.{nameof(CustardEntity.Id)} = {TableNames.CustardEntitiesName}.{nameof(CustardEntity.Id)};
            """;
            int totalUpdated = await _context.Database.ExecuteSqlRawAsync(updateSql, ct);

            // Insert new rows that do not exist yet
            string insertSql = $"""
                INSERT INTO {TableNames.CustardEntitiesName} (
                    {nameof(CustardEntity.Id)},
                    {nameof(CustardEntity.Active)}, 
                    {nameof(CustardEntity.PhoneNumber)}, 
                    {nameof(CustardEntity.PhoneNumber2)}, 
                    {nameof(CustardEntity.Date)}, 
                    {nameof(CustardEntity.UnixDate)}, 
                    {nameof(CustardEntity.CancelDate)}, 
                    {nameof(CustardEntity.UnixCancelDate)}
                )
                SELECT 
                    temp.{nameof(CustardEntity.Id)},
                    temp.{nameof(CustardEntity.Active)}, 
                    temp.{nameof(CustardEntity.PhoneNumber)}, 
                    temp.{nameof(CustardEntity.PhoneNumber2)}, 
                    temp.{nameof(CustardEntity.Date)}, 
                    temp.{nameof(CustardEntity.UnixDate)}, 
                    temp.{nameof(CustardEntity.CancelDate)}, 
                    temp.{nameof(CustardEntity.UnixCancelDate)}
                FROM {tempTable} temp
                WHERE NOT EXISTS (
                    SELECT 1 
                    FROM {TableNames.CustardEntitiesName} t
                    WHERE t.{nameof(CustardEntity.Id)} = temp.{nameof(CustardEntity.Id)}
                );
            """;
            int totalInserted = await _context.Database.ExecuteSqlRawAsync(insertSql, ct);


            await _context.Database.ExecuteSqlRawAsync($"DELETE FROM {tempTable};", ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Unique={Unique}, Staged={Staged}, Updated={Updated}, Inserted={Inserted}, Skipped={Skipped}",
                nameof(CustardEntity),
                entities.Count,
                uniqueEntities.Count,
                stagedCount,
                totalUpdated,
                totalInserted,
                skipped);

            return Result.Success(uniqueEntities);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Entity} upsert failed.", 
                nameof(CustardEntity));
            return Result.Failure<List<CustardEntity>>(ex.ToString());
        }

        void InsertBatch(List<CustardEntity> batch)
        {
            var values = new List<object>();
            var rows = new List<string>();
            const int colsPerRow = 8;

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];
                int offset = i * colsPerRow;

                rows.Add($"({{{offset}}}, {{{offset + 1}}}, {{{offset + 2}}}, {{{offset + 3}}}, {{{offset + 4}}}, {{{offset + 5}}}, {{{offset + 6}}}, {{{offset + 7}}})");

                values.Add(e.Id);
                values.Add(e.Active ? 1 : 0);
                values.Add(e.PhoneNumber.Number);
                values.Add(e.PhoneNumber2?.Number ?? (object)DBNull.Value); // Safe null handling
                values.Add(e.Date.ToString("yyyy-MM-dd HH:mm:ss"));
                values.Add(e.UnixDate);

                // Handle potentially uninitialized or null DateTime
                values.Add(e.CancelDate == default ? (object)DBNull.Value : e.CancelDate.ToString("yyyy-MM-dd HH:mm:ss"));
                values.Add(e.UnixCancelDate);
            }

            string sql = $"INSERT INTO {tempTable} VALUES {string.Join(",", rows)};";
            _context.Database.ExecuteSqlRaw(sql, [.. values]);
        }
    }

}
