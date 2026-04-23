using CSharpFunctionalExtensions;
using LeadPipe.Core;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Repository;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public class SyncStateRepository(PlumbingContext context) : ISyncStateRepository
{
    private readonly PlumbingContext _context = context;
    private readonly DbSet<SyncStateEntity> _set = context.Set<SyncStateEntity>();

    public async Task<Result<SyncStateEntity>> GetByKeyAsync(Source? source, SyncKey key)
    {
        BusinessId id = BusinessId.BuildBusinessId(source, key);
        Result<SyncStateEntity> result = await GetByIdAsync(id);
        return result;
    }

    public async Task<Result<SyncStateEntity>> GetByIdAsync(BusinessId id)
    {
        try
        {
            SyncStateEntity? entity = await _set
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.BusinessId == id);

            if (entity is null)
                return Result.Failure<SyncStateEntity>(
                    $"{nameof(SyncStateEntity)} not found for {nameof(BusinessId)} '{id}'");

            return Result.Success(entity);
        }
        catch (Exception ex)
        {
            return Result.Failure<SyncStateEntity>(
                $"Failed to retrieve {nameof(SyncStateEntity)} for {nameof(BusinessId)} '{id}': {ex}");
        }
    }

    public async Task<Result<List<SyncStateEntity>>> UpsertRangeAsync(List<SyncStateEntity> entities)
    {
        if (entities is null || entities.Count == 0)
            return Result.Success(new List<SyncStateEntity>());

        try
        {
            // Pull all existing rows for the incoming business IDs in one round-trip
            List<BusinessId> businessIds = [.. entities.Select(e => e.BusinessId)];

            List<SyncStateEntity> existing = await _set
                .Where(x => businessIds.Contains(x.BusinessId))
                .ToListAsync();

            Dictionary<BusinessId, SyncStateEntity> existingByBusinessId = existing
                .ToDictionaryFast(x => x.BusinessId);

            foreach (var incoming in entities)
            {
                if (existingByBusinessId.TryGetValue(incoming.BusinessId, out var tracked))
                {
                    // Update tracked entity
                    tracked.LastProcessedId = incoming.LastProcessedId;
                    tracked.LastSyncUtc = incoming.LastSyncUtc;
                    tracked.UnixLastSyncUtc = incoming.UnixLastSyncUtc;
                }
                else
                {
                    await _set.AddAsync(incoming);
                }
            }

            await _context.SaveChangesAsync();

            return Result.Success(entities);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<SyncStateEntity>>(
                $"Failed to upsert {nameof(SyncStateEntity)} range: {ex}");
        }
    }
}
