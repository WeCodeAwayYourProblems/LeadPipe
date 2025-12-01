using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.MySql.Context;
using LeadPipe.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.MySql.Repository;

public class CustomerCallMySqlRepository(MySqlContext context) : ICustomerCallMySqlRepository
{
    private readonly MySqlContext _context = context;
    private readonly DbSet<CustomerCallMySqlEntity> _set = context.Set<CustomerCallMySqlEntity>();

    public async Task<Result<CustomerCallMySqlEntity>> AddAsync(CustomerCallMySqlEntity entity)
    {
        await _set.AddAsync(entity);
        await _context.SaveChangesAsync();
        return Result.Success(entity);
    }

    public async Task<Result<List<CustomerCallMySqlEntity>>> AddRangeAsync(List<CustomerCallMySqlEntity> entities)
    {
        if (entities is null || entities.Count == 0)
            return Result.Failure<List<CustomerCallMySqlEntity>>("No entities provided.");

        await _set.AddRangeAsync(entities);
        await _context.SaveChangesAsync();
        return Result.Success(entities);
    }

    public async Task<Result<bool>> DeleteAsync(long id)
    {
        var entity = await _set.FindAsync((long)id);
        if (entity is null)
            return Result.Success(false);

        _set.Remove(entity);
        await _context.SaveChangesAsync();
        return Result.Success(true);
    }

    public async Task<Result<bool>> DeleteAsync(CustomerCallMySqlEntity entity)
        => await DeleteAsync(entity.Id);

    public async Task<Result<List<CustomerCallMySqlEntity>>> GetAllAsync()
        => Result.Success(await _set.ToListAsync());

    public async Task<Result<CustomerCallMySqlEntity>> GetByIdAsync(long id)
    {
        var found = await _set.FindAsync(id);
        return found is null
            ? Result.Failure<CustomerCallMySqlEntity>($"Entity with id {id} was not found")
            : Result.Success(found);
    }

    public async Task<Result<CustomerCallMySqlEntity>> UpdateAsync(CustomerCallMySqlEntity entity)
    {
        var exists = await _set.FindAsync(entity.Id);
        if (exists is null)
            return Result.Failure<CustomerCallMySqlEntity>("The desired entity does not exist");

        _context.Entry(exists).CurrentValues.SetValues(entity);
        await _context.SaveChangesAsync();
        return Result.Success(entity);
    }
}

