using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces.Repository.MySql;
using LeadPipe.Infrastructure.MySql.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LeadPipe.Infrastructure.MySql.Repository;

public class SummaryMySqlRepository(MySqlContext context) : ISummaryMySqlRepository
{
    private readonly MySqlContext _context = context;
    private readonly DbSet<SummaryMySqlEntity> _set = context.Set<SummaryMySqlEntity>();

    public async Task<Result<List<SummaryMySqlEntity>>> FindAsync(Expression<Func<SummaryMySqlEntity, bool>> predicate)
    {
        try
        {
            var list = await _set.Where(predicate).ToListAsync();
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<SummaryMySqlEntity>>(ex.Message);
        }
    }
    public async Task<Result<SummaryMySqlEntity>> AddAsync(SummaryMySqlEntity entity)
    {
        await _set.AddAsync(entity);
        await _context.SaveChangesAsync();
        return Result.Success(entity);
    }

    public async Task<Result<List<SummaryMySqlEntity>>> AddRangeAsync(List<SummaryMySqlEntity> entities)
    {
        if (entities is null || entities.Count == 0)
            return Result.Failure<List<SummaryMySqlEntity>>("No entities provided.");

        await _set.AddRangeAsync(entities);
        await _context.SaveChangesAsync();
        return Result.Success(entities);
    }

    public async Task<Result<bool>> DeleteAsync(long id)
    {
        var entity = await _set.FindAsync(id); // PK = call_id
        if (entity is null)
            return Result.Success(false);

        _set.Remove(entity);
        await _context.SaveChangesAsync();
        return Result.Success(true);
    }

    public async Task<Result<bool>> DeleteAsync(SummaryMySqlEntity entity)
        => await DeleteAsync(entity.call_id);

    public async Task<Result<List<SummaryMySqlEntity>>> GetAllAsync()
        => Result.Success(await _set.ToListAsync());

    public async Task<Result<SummaryMySqlEntity>> GetByIdAsync(long id)
    {
        var found = await _set.FindAsync(id);
        return found is null
            ? Result.Failure<SummaryMySqlEntity>($"Entity with id {id} was not found")
            : Result.Success(found);
    }

    public async Task<Result<SummaryMySqlEntity>> UpdateAsync(SummaryMySqlEntity entity)
    {
        var exists = await _set.FindAsync(entity.call_id);
        if (exists is null)
            return Result.Failure<SummaryMySqlEntity>("The desired entity does not exist");

        _context.Entry(exists).CurrentValues.SetValues(entity);
        await _context.SaveChangesAsync();
        return Result.Success(entity);
    }
}
