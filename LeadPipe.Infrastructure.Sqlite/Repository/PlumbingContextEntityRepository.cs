using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public abstract class PlumbingContextEntityRepository<TEntity, TRepo>
    (
        PlumbingContext context,
        ILogger<TRepo> logger
    ) : PlumbingContextBaseRepository<TEntity, TRepo>(context, logger)
    where TEntity : class, IEntity
{
    protected record UpsertFields(string TableName, string TempTable, string EntityName, int ColumnCount);
    protected abstract UpsertFields EntityDetails { get; }
    protected virtual string DropTempTable => $"DROP TABLE IF EXISTS {EntityDetails.TempTable};";
    protected abstract string? Type { get; set; }
    protected abstract string CreateTempTable { get; }
    protected abstract string UpdateSql { get; }
    protected abstract string InsertSql { get; }
    protected abstract bool IsUpdatable { get; }
    protected abstract int[] ColumnIndexes { get; }
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

        int safeMax = ParameterLimit / EntityDetails.ColumnCount;
        int currentBatchSize = safeMax;
        const int minBatchSize = 1;
        int stagedCount = 0;
        int skipped = 0;

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);
            await _context.Database.ExecuteSqlRawAsync(DropTempTable, ct);
            await _context.Database.ExecuteSqlRawAsync(CreateTempTable, ct);

            for (int index = 0; index < entities.Count;)
            {
                int take = Math.Min(currentBatchSize, entities.Count - index);
                var batch = entities.GetRange(index, take);

                try
                {
                    InsertBatch(batch);
                    stagedCount += batch.Count;
                    index += take; // manual increment only

                    if (currentBatchSize < safeMax)
                        currentBatchSize = Math.Min(currentBatchSize * 2, safeMax);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "{Entity} batch insert failed (size={BatchSize}). Reducing batch size.",
                        EntityDetails.EntityName,
                        currentBatchSize);

                    if (currentBatchSize == minBatchSize)
                    {
                        skipped++;
                        index++;
                        currentBatchSize = safeMax;
                    }
                    else
                    {
                        currentBatchSize = Math.Max(minBatchSize, currentBatchSize / 2);
                    }
                }
            }

            // Update
            int updatedCount = IsUpdatable
                ? await _context.Database.ExecuteSqlRawAsync(UpdateSql, ct)
                : 0;

            // Insert new rows
            int insertedCount = await _context.Database.ExecuteSqlRawAsync(InsertSql, ct);

            string delete = $"DELETE FROM {EntityDetails.TempTable};";
            await _context.Database.ExecuteSqlRawAsync(delete, ct);

            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Type={Type}, IsUpdatable={IsUpdatable}, Incoming={Incoming}, Staged={Staged}, Updated={Updated}, Inserted={Inserted}, Skipped={Skipped}",
                EntityDetails.EntityName,
                Type,
                IsUpdatable,
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