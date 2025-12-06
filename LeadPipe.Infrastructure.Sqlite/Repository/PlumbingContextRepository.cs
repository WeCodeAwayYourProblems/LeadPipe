using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public abstract class PlumbingContextRepository<T>(PlumbingContext context) : IRepository<T> where T : class, IEntity
{
    protected readonly PlumbingContext _context = context;
    protected readonly DbSet<T> _set = context.Set<T>();

    #region Implementation
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

    public async Task<Result<List<T>>> AddRangeAsync(List<T> entities)
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

    public async Task<Result<List<T>>> GetAllAsync()
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
    #endregion
}