using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces.Repository.MySql;
using LeadPipe.Infrastructure.MySql.Context;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.MySql.Repository;

public class CallMySqlRepository(MySqlContext context) : ICallMySqlRepository
{
    private readonly MySqlContext _context = context;
    private readonly DbSet<CallMySqlEntity> _set = context.Set<CallMySqlEntity>();

    public async Task<Result<CallMySqlEntity>> AddAsync(CallMySqlEntity entity)
    {
        await _set.AddAsync(entity);
        await _context.SaveChangesAsync();
        return Result.Success(entity);
    }

    public async Task<Result<List<CallMySqlEntity>>> AddRangeAsync(List<CallMySqlEntity> entities)
    {
        if (entities is null || entities.Count == 0)
            return Result.Failure<List<CallMySqlEntity>>("No entities provided.");

        await _set.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        return Result.Success(entities);
    }

    public async Task<Result<bool>> DeleteAsync(long id)
    {
        var entity = await _set.FindAsync(id);
        if (entity is null)
            return Result.Success(false);

        _set.Remove(entity);
        await _context.SaveChangesAsync();

        return Result.Success(true);
    }

    public async Task<Result<bool>> DeleteAsync(CallMySqlEntity entity)
        => await DeleteAsync(entity.call_id);

    public async Task<Result<List<CallMySqlEntity>>> GetAllAsync()
        => Result.Success(await _set.ToListAsync());

    public async Task<Result<CallMySqlEntity>> GetByIdAsync(long id)
    {
        var found = await _set.FindAsync(id);
        return found is null
            ? Result.Failure<CallMySqlEntity>($"Entity with id {id} was not found")
            : Result.Success(found);
    }

    public async Task<Result<CallMySqlEntity>> UpdateAsync(CallMySqlEntity entity)
    {
        var exists = await _set.FindAsync(entity.call_id);
        if (exists is null)
            return Result.Failure<CallMySqlEntity>("The desired entity does not exist");

        _context.Entry(exists).CurrentValues.SetValues(entity);
        await _context.SaveChangesAsync();
        return Result.Success(entity);
    }
}

