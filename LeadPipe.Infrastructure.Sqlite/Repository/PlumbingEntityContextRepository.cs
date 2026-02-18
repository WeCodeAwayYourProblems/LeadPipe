using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public abstract class PlumbingEntityContextRepository<TEntity, TRepo>
    (
        PlumbingContext context,
        ILogger<TRepo> logger
    ) : PlumbingContextRepository<TEntity, TRepo>(context, logger)
    where TEntity : class, IEntity
{
    protected record UpsertFields(string TableName, string TempTable, string EntityName, int ColumnCount);
    protected abstract UpsertFields EntityDetails { get; }
    protected abstract string CreateTempTable { get; }
    protected abstract string UpdateSql { get; }
    protected abstract string InsertSql { get; }
    protected abstract void InsertBatch(List<TEntity> batch);

    internal async Task<Result<List<TEntity>>> UpsertEntityRangeAsync(
        List<TEntity> entities,
        CancellationToken ct = default)
    {
        if (entities.Count == 0)
        {
            _logger.LogInformation(
                "{Entity} upsert reverted. No entities entered. Returning success state.",
                EntityDetails.EntityName
            );
            return Result.Success(entities);
        }

        int batchSize = 999 / EntityDetails.ColumnCount;
        const int minBatchSize = 1;
        int stagedCount = 0;
        int skipped = 0;

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            await _context.Database.ExecuteSqlRawAsync(CreateTempTable, ct);

            for (int index = 0; index < entities.Count; index++)
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
                        EntityDetails.EntityName,
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
            int updatedCount = await _context.Database.ExecuteSqlRawAsync(UpdateSql, ct);

            // Insert new rows
            int insertedCount = await _context.Database.ExecuteSqlRawAsync(InsertSql, ct);

            string delete = $"DELETE FROM {EntityDetails.TempTable};";
            await _context.Database.ExecuteSqlRawAsync(delete, ct);

            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Staged={Staged}, Updated={Updated}, Inserted={Inserted}, Skipped={Skipped}",
                EntityDetails.EntityName,
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
                EntityDetails.EntityName);
            return Result.Failure<List<TEntity>>(ex.ToString());
        }

    }

}