using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.MySql.Context;
using LeadPipe.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.MySql.Repository;

public class CustardMySqlRepository(MySqlContext context) : ICustardMySqlRepository
{
    private readonly MySqlContext _context = context;
    private readonly DbSet<CustardMySqlEntity> _set = context.Set<CustardMySqlEntity>();

    public async Task<Result<CustardMySqlEntity>> AddAsync(CustardMySqlEntity entity)
    {
        await _set.AddAsync(entity);
        await _context.SaveChangesAsync();
        return Result.Success(entity);
    }

    public async Task<Result<List<CustardMySqlEntity>>> AddRangeAsync(List<CustardMySqlEntity> entities)
    {
        if (entities is null || entities.Count == 0)
            return Result.Failure<List<CustardMySqlEntity>>("No entities provided.");

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

    public async Task<Result<bool>> DeleteAsync(CustardMySqlEntity entity)
        => await DeleteAsync(entity.customerID);

    public async Task<Result<List<CustardMySqlEntity>>> GetAllAsync()
        => Result.Success(await _set.ToListAsync());

    public async Task<Result<CustardMySqlEntity>> GetByIdAsync(long id)
    {
        var found = await _set.FindAsync((int)id);
        return found is null
            ? Result.Failure<CustardMySqlEntity>($"Entity with id {id} was not found")
            : Result.Success(found);
    }

    public async Task<Result<CustardMySqlEntity>> UpdateAsync(CustardMySqlEntity entity)
    {
        var exists = await _set.FindAsync(entity.customerID);
        if (exists is null)
            return Result.Failure<CustardMySqlEntity>("The desired entity does not exist");

        _context.Entry(exists).CurrentValues.SetValues(entity);
        await _context.SaveChangesAsync();
        return Result.Success(entity);
    }
}

