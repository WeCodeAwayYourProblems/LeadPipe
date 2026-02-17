using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class CornRepository(
    PlumbingContext context,
    ILogger<CornRepository> logger
) : PlumbingContextRepository<CornEntity, CornRepository>(context, logger), IRepository<CornEntity>
{
    protected override IQueryable<CornEntity> WithIncludes(IQueryable<CornEntity> q)
    {
        return q
            .Include(c => c.CustardCornLinks)
            .Include(c => c.SandCornLinks)
            .Include(c => c.CornCaliperLinks)
            .Include(c => c.CornPlumbingLinks);
    }

    public override async Task<Result<List<CornEntity>>> UpsertRangeAsync(
        List<CornEntity> entities,
        CancellationToken ct = default)
    {
        if (entities.Count == 0)
            return Result.Success(new List<CornEntity>());

        AssertNotString<CornEntity>(nameof(CornEntity.PhoneNumber));
        AssertNotString<CornEntity>(nameof(CornEntity.Date));
        AssertNotString<CornEntity>(nameof(CornEntity.UnixDate));

        int batchSize = 200;
        const int minBatchSize = 1;
        int stagedCount = 0;
        int skipped = 0;
        const string tempTable = "temp_corn";

        entities =
        [
            .. entities
                .GroupBy(e => e.Id)
                .Select(g => g.Last())
        ];

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            await _context.Database.ExecuteSqlRawAsync($"""
                CREATE TEMP TABLE IF NOT EXISTS {tempTable} (
                    {nameof(CornEntity.Id)} INTEGER PRIMARY KEY,
                    {nameof(CornEntity.PhoneNumber)} INTEGER NOT NULL,
                    {nameof(CornEntity.Date)} TEXT NOT NULL,
                    {nameof(CornEntity.UnixDate)} INTEGER NOT NULL,
                    {nameof(CornEntity.Payload)} TEXT NOT NULL,
                    {nameof(CornEntity.MetaData)} TEXT NOT NULL,
                    {nameof(CornEntity.Source)} TEXT NOT NULL
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
                        nameof(CornEntity),
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

            // Update existing rows
            string updateSql = $"""
                UPDATE {TableNames.CornEntitiesName}
                SET 
                    {nameof(CornEntity.PhoneNumber)} = temp.{nameof(CornEntity.PhoneNumber)},
                    {nameof(CornEntity.Date)} = temp.{nameof(CornEntity.Date)},
                    {nameof(CornEntity.UnixDate)} = temp.{nameof(CornEntity.UnixDate)},
                    {nameof(CornEntity.Payload)} = temp.{nameof(CornEntity.Payload)},
                    {nameof(CornEntity.MetaData)} = temp.{nameof(CornEntity.MetaData)},
                    {nameof(CornEntity.Source)} = temp.{nameof(CornEntity.Source)}
                FROM {tempTable} temp
                WHERE {TableNames.CornEntitiesName}.{nameof(CornEntity.Id)} = temp.{nameof(CornEntity.Id)};
            """;
            int totalUpdated = await _context.Database.ExecuteSqlRawAsync(updateSql, ct);

            // Insert new rows that do not exist yet
            string insertSql = $"""
                INSERT INTO {TableNames.CornEntitiesName} 
                (
                    {nameof(CornEntity.Id)}, 
                    {nameof(CornEntity.PhoneNumber)}, 
                    {nameof(CornEntity.Date)}, 
                    {nameof(CornEntity.UnixDate)}, 
                    {nameof(CornEntity.Payload)}, 
                    {nameof(CornEntity.MetaData)},  
                    {nameof(CornEntity.Source)}
                )
                SELECT 
                    temp.{nameof(CornEntity.Id)}, 
                    temp.{nameof(CornEntity.PhoneNumber)}, 
                    temp.{nameof(CornEntity.Date)}, 
                    temp.{nameof(CornEntity.UnixDate)}, 
                    temp.{nameof(CornEntity.Payload)},     
                    temp.{nameof(CornEntity.MetaData)}, 
                    temp.{nameof(CornEntity.Source)}
                FROM {tempTable} temp
                WHERE NOT EXISTS (
                    SELECT 1 
                    FROM {TableNames.CornEntitiesName} t
                    WHERE t.{nameof(CornEntity.Id)} = temp.{nameof(CornEntity.Id)}
                );
            """;
            int totalInserted = await _context.Database.ExecuteSqlRawAsync(insertSql, ct);

            await _context.Database.ExecuteSqlRawAsync($"DELETE FROM {tempTable};", ct);

            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Staged={Staged}, Updated={Updated}, Inserted={Inserted}, Skipped={Skipped}",
                nameof(CornEntity),
                entities.Count,
                stagedCount,
                totalUpdated,
                totalInserted,
                skipped);

            return Result.Success(entities);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Entity} upsert failed.",
                nameof(CornEntity));
            return Result.Failure<List<CornEntity>>(ex.ToString());
        }

        void InsertBatch(List<CornEntity> batch)
        {
            var values = new List<object>();
            var rows = new List<string>();
            const int colsPerRow = 7;

            for (int i = 0; i < batch.Count; i++)
            {
                var e = batch[i];
                int offset = i * colsPerRow;

                // Build placeholder string: ({0}, {1}, {2}, {3}, {4}, {5}, {6})
                rows.Add($"({{{offset}}}, {{{offset + 1}}}, {{{offset + 2}}}, {{{offset + 3}}}, {{{offset + 4}}}, {{{offset + 5}}}, {{{offset + 6}}})");

                values.Add(e.Id);
                values.Add(e.PhoneNumber.Number);
                values.Add(e.Date.ToString("yyyy-MM-dd HH:mm:ss"));
                values.Add(e.UnixDate);
                values.Add(e.Payload ?? string.Empty);
                values.Add(e.MetaData ?? string.Empty);
                values.Add(e.Source ?? string.Empty);
            }

            string sql = $"INSERT INTO {tempTable} VALUES {string.Join(",", rows)};";
            _context.Database.ExecuteSqlRaw(sql, [.. values]);
        }
    }

}
