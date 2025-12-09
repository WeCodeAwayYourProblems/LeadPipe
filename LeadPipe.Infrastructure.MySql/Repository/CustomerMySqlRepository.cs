using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces.Repository.MySql;
using LeadPipe.Infrastructure.MySql.Context;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.MySql.Repository;

public class CustomerMySqlRepository(MySqlContext context) : ICustomerMySqlRepository
{
    private readonly MySqlContext _context = context;
    private readonly DbSet<CustomerMySqlEntity> _set = context.Set<CustomerMySqlEntity>();

    public async Task<Result<CustomerMySqlEntity>> AddAsync(CustomerMySqlEntity entity)
    {
        await _set.AddAsync(entity);
        await _context.SaveChangesAsync();
        return Result.Success(entity);
    }

    public async Task<Result<List<CustomerMySqlEntity>>> AddRangeAsync(List<CustomerMySqlEntity> entities)
    {
        if (entities is null || entities.Count == 0)
            return Result.Failure<List<CustomerMySqlEntity>>("No entities provided.");

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

    public async Task<Result<bool>> DeleteAsync(CustomerMySqlEntity entity)
        => await DeleteAsync(entity.customerID);

    public async Task<Result<List<CustomerMySqlEntity>>> GetAllAsync()
        => Result.Success(await _set.ToListAsync());

    public async Task<Result<CustomerMySqlEntity>> GetByIdAsync(long id)
    {
        var found = await _set.FindAsync((int)id);
        return found is null
            ? Result.Failure<CustomerMySqlEntity>($"Entity with id {id} was not found")
            : Result.Success(found);
    }

    public async Task<Result<CustomerMySqlEntity>> UpdateAsync(CustomerMySqlEntity entity)
    {
        var exists = await _set.FindAsync(entity.customerID);
        if (exists is null)
            return Result.Failure<CustomerMySqlEntity>("The desired entity does not exist");

        _context.Entry(exists).CurrentValues.SetValues(entity);
        await _context.SaveChangesAsync();
        return Result.Success(entity);
    }
}
