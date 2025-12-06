using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces.Repository;
using LeadPipe.Infrastructure.MySql.Context;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.MySql.Repository;

public class SubMySqlRepository(MySqlContext context) : ISubMySqlRepository
{
    private readonly MySqlContext _context = context;
    private readonly DbSet<SubMySqlEntity> _set = context.Set<SubMySqlEntity>();

    public async Task<Result<SubMySqlEntity>> AddAsync(SubMySqlEntity entity)
    {
        await _set.AddAsync(entity);
        await _context.SaveChangesAsync();
        return Result.Success(entity);
    }

    public async Task<Result<List<SubMySqlEntity>>> AddRangeAsync(List<SubMySqlEntity> entities)
    {
        if (entities is null || entities.Count == 0)
            return Result.Failure<List<SubMySqlEntity>>("No entities provided.");

        await _set.AddRangeAsync(entities);
        await _context.SaveChangesAsync();
        return Result.Success(entities);
    }

    public async Task<Result<bool>> DeleteAsync(long id)
    {
        var entity = await _set.FindAsync((int)id);
        if (entity is null)
            return Result.Success(false);

        _set.Remove(entity);
        await _context.SaveChangesAsync();
        return Result.Success(true);
    }

    public async Task<Result<bool>> DeleteAsync(SubMySqlEntity entity)
        => await DeleteAsync(entity.subscriptionID);

    public async Task<Result<List<SubMySqlEntity>>> GetAllAsync()
        => Result.Success(await _set.ToListAsync());

    public async Task<Result<SubMySqlEntity>> GetByIdAsync(long id)
    {
        var found = await _set.FindAsync((int)id);
        return found is null
            ? Result.Failure<SubMySqlEntity>($"Entity with id {id} was not found")
            : Result.Success(found);
    }

    public async Task<Result<SubMySqlEntity>> UpdateAsync(SubMySqlEntity entity)
    {
        var exists = await _set.FindAsync(entity.subscriptionID);
        if (exists is null)
            return Result.Failure<SubMySqlEntity>("The desired entity does not exist");

        _context.Entry(exists).CurrentValues.SetValues(entity);
        await _context.SaveChangesAsync();
        return Result.Success(entity);
    }
}

