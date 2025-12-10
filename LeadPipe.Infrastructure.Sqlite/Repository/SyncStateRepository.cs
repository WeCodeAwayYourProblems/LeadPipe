using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public class SyncStateRepository(PlumbingContext context) : ISyncStateRepository
{
    private readonly PlumbingContext _context = context;
    private readonly DbSet<SyncStateEntity> _set = context.Set<SyncStateEntity>();

    public async Task<Result<SyncStateEntity>> GetAsync()
    {
        try
        {
            var entity = await _set.FirstOrDefaultAsync();

            if (entity is null)
            {
                // Always use a fixed ID
                entity = new SyncStateEntity
                {
                    LastProcessedId = "",
                    LastSyncUtc = DateTime.UtcNow,
                    UnixLastSyncUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };

                await _set.AddAsync(entity);
                await _context.SaveChangesAsync();
            }

            return Result.Success(entity);
        }
        catch (Exception ex)
        {
            return Result.Failure<SyncStateEntity>($"Failed to retrieve SyncState: {ex.Message}");
        }
    }

    public async Task<Result<SyncStateEntity>> SaveAsync(SyncStateEntity updated)
    {
        try
        {
            SyncStateEntity? existing = await _set.FirstOrDefaultAsync();

            if (existing is null)
            {
                // First-time insert
                await _set.AddAsync(updated);
            }
            else
            {
                // Update existing tracked entity
                existing.LastProcessedId = updated.LastProcessedId;
                existing.LastSyncUtc = updated.LastSyncUtc;
                existing.UnixLastSyncUtc = updated.UnixLastSyncUtc;
            }

            await _context.SaveChangesAsync();

            // Return the entity actually saved
            return Result.Success(existing ?? updated);
        }
        catch (Exception ex)
        {
            return Result.Failure<SyncStateEntity>($"Failed to save SyncState: {ex.Message}");
        }
    }
}
