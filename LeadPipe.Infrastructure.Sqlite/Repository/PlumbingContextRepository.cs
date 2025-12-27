using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Linq.Expressions;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public abstract class PlumbingContextRepository<T, T2>(PlumbingContext context, ILogger<T2> logger)
    : IRepository<T> where T : class, IEntity
{
    protected readonly PlumbingContext _context = context;
    protected readonly DbSet<T> _set = context.Set<T>();
    internal readonly ILogger<T2> _logger = logger;
    public async Task<Result<List<T>>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            var list = await _set.Where(predicate).ToListAsync();
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<T>>(ex.Message);
        }
    }

    public async Task<Result<T>> AddAsync(T entity)
    {
        try
        {
            await _set.AddAsync(entity);
            await _context.SaveChangesAsync();
            return Result.Success(entity);
        }
        catch (Exception ex)
        {
            return Result.Failure<T>($"Failed to save entities: {ex.Message}");
        }
    }

    public virtual async Task<Result<List<T>>> AddRangeAsync(List<T> entities)
    {
        if (entities is null || entities.Count == 0)
            return Result.Failure<List<T>>("No entities provided.");

        try
        {
            await _set.AddRangeAsync(entities);
            await _context.SaveChangesAsync();

            return Result.Success(entities);
        }
        catch (Exception ex)
        {
            // Catch any exception and wrap it into a Result.Failure
            return Result.Failure<List<T>>($"Failed to save entities: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteAsync(long id)
    {
        T? entity = await _set.FindAsync(id);
        if (entity is null)
            return Result.Success(false);

        try
        {
            _set.Remove(entity);
            await _context.SaveChangesAsync();

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>($"Failed to Delete entity: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteAsync(T entity)
    {
        return await DeleteAsync(entity.Id);
    }

    public virtual async Task<Result<List<T>>> GetAllAsync()
    {
        try
        {
            var list = await _set.ToListAsync();
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<T>>($"Failed to get entities: {ex.Message}");
        }
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

        try
        {

            // Update
            _context.Entry(exists).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            return Result.Success(entity);
        }
        catch (Exception ex)
        {
            return Result.Failure<T>($"Failed to update entity:\n{ex.Message}");
        }
    }

    public async Task<Result<T>> UpsertAsync(T entity)
    {
        try
        {
            T? existing = await _set.FindAsync(entity.Id);

            if (existing is null)
            {
                await _set.AddAsync(entity);
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
            }

            await _context.SaveChangesAsync();
            return Result.Success(entity);
        }
        catch (Exception ex)
        {
            return Result.Failure<T>(ex.Message);
        }
    }

    public virtual async Task<Result<List<T>>> UpsertRangeAsync(List<T> entities)
    {
        throw new NotSupportedException(
        $"{typeof(T).Name} must implement a SQLite-native UpsertRangeAsync.");
    }
}

