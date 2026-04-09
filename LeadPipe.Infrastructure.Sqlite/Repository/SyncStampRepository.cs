using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public class SyncStampRepository(PlumbingContext context) : ISyncStampRepository
{
    private readonly PlumbingContext _context = context;
    private readonly DbSet<SyncStampEntity> _set = context.Set<SyncStampEntity>();

    public async Task<Result<SyncStampEntity>> GetByKeyAsync(Source? source, SyncKey key)
    {
        try
        {
            SyncStampEntity? entity = await _set
                .AsNoTracking()
                .SingleOrDefaultAsync(Predicate(source, key));

            if (entity is null)
                return Result.Failure<SyncStampEntity>($"{nameof(SyncStampEntity)} not found for {nameof(SyncKey)} '{key}'");

            return Result.Success(entity);
        }
        catch (Exception ex) { return Result.Failure<SyncStampEntity>($"Failed to retrieve {nameof(SyncStampEntity)} for {nameof(SyncKey)} '{key}'. Exception: {ex}"); }

        static Expression<Func<SyncStampEntity, bool>> Predicate(Source? source, SyncKey key) => x => x.Key == key && x.Source == source;
    }

    public async Task<Result<SyncStampEntity>> UpsertAsync(SyncStampEntity entity)
    {
        try
        {
            // Check whether the input entity exists in the database
            SyncStampEntity? existing = await _set
                .FirstOrDefaultAsync(Predicate(entity));

            if (existing is null)
                await _set.AddAsync(entity);
            else
            {
                existing.SuccessState = entity.SuccessState;
                existing.UnixSyncUtc = entity.UnixSyncUtc;
            }

            await _context.SaveChangesAsync();

            return Result.Success(existing ?? entity);
        }
        catch (Exception ex) { return Result.Failure<SyncStampEntity>($"Failed to upsert {nameof(SyncStampEntity)}. Exception: {ex}"); }

        static System.Linq.Expressions.Expression<Func<SyncStampEntity, bool>> Predicate(SyncStampEntity entity)
        {
            return entity.Id == default
                ? x => x.Key == entity.Key && x.Source == entity.Source
                : x => x.Id == entity.Id;
        }
    }
}
