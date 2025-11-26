using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Database;
using LeadPipe.Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.Repository;

#region Plumbing Repositories
public interface ICallRepository : IRepository<CallEntity> { }
internal class CallRepository(PlumbingContext context) : PlumbingContextRepository<CallEntity>(context), ICallRepository { }
public interface IPlumbingCallLinkRepository : IRepository<PlumbingCallLink> { }
internal class PlumbingCallLinkRepository(PlumbingContext context) : PlumbingContextRepository<PlumbingCallLink>(context), IPlumbingCallLinkRepository { }
public interface IPlumbingRepository : IRepository<PlumbingEntity> { }
internal class PlumbingRepository(PlumbingContext context) : PlumbingContextRepository<PlumbingEntity>(context), IPlumbingRepository { }
public interface ISubsCallLinkRepository : IRepository<SubsCallLink> { }
internal class SubsCallLinkRepository(PlumbingContext context) : PlumbingContextRepository<SubsCallLink>(context), ISubsCallLinkRepository { }
public interface ISubsRepository : IRepository<SubsEntity> { }
internal class SubsRepository(PlumbingContext context) : PlumbingContextRepository<SubsEntity>(context), ISubsRepository { }
public interface ISubsPlumbingLinkRepository : IRepository<SubsPlumbingLink> { }
internal class SubsPlumbingLinkRepository(PlumbingContext context) : PlumbingContextRepository<SubsPlumbingLink>(context), ISubsPlumbingLinkRepository { }
#endregion

internal abstract class PlumbingContextRepository<T>(PlumbingContext context) : IRepository<T> where T : class, IEntity
{
    protected readonly PlumbingContext _context = context;
    protected readonly DbSet<T> _set = context.Set<T>();

    #region Implementation
    public async Task<Result<T>> AddAsync(T entity)
    {
        await _set.AddAsync(entity);
        await _context.SaveChangesAsync();
        return Result.Success(entity);
    }

    public async Task<Result<List<T>>> AddRangeAsync(List<T> entities)
    {
        if (entities is null || entities.Count == 0)
            return Result.Failure<List<T>>("No entities provided.");

        await _set.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        return Result.Success(entities);
    }

    public async Task<Result<bool>> DeleteAsync(long id)
    {
        T? entity = await _set.FindAsync(id);
        if (entity is null)
            return Result.Success(false);

        _set.Remove(entity);
        await _context.SaveChangesAsync();

        return Result.Success(true);
    }

    public async Task<Result<bool>> DeleteAsync(T entity)
    {
        return await DeleteAsync(entity.Id);
    }

    public async Task<Result<List<T>>> GetAllAsync()
    {
        var list = await _set.ToListAsync();
        return Result.Success(list);
    }

    public async Task<Result<T>> GetByIdAsync(long id)
    {
        T? found = await _set.FindAsync(id);
        return found is null
            ? Result.Failure<T>($"Entity with id {id} was not found")
            : Result.Success(found);
    }

    public async Task<Result<T>> UpdateAsync(T entity)
    {
        // Check for existence
        T? exists = await _set.FindAsync(entity.Id);
        if (exists is null)
            return Result.Failure<T>("The desired entity does not exist");

        // Update
        _context.Entry(exists).CurrentValues.SetValues(entity);
        await _context.SaveChangesAsync();
        return Result.Success(entity);
    }
    #endregion
}