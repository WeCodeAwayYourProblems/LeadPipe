using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Repository.MySql;
using LeadPipe.Infrastructure.MySql.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LeadPipe.Infrastructure.MySql.Repository;

public class CustardMySqlRepository(MySqlSchema1Context context) : ICustardMySqlRepository
{
    private readonly DbSet<CustardMySqlEntity> _set = context.Set<CustardMySqlEntity>();

    public async Task<Result<List<CustardMySqlEntity>>> FindAsync(Expression<Func<CustardMySqlEntity, bool>> predicate, bool includeSubscriptions = true)
    {
        try
        {
            IQueryable<CustardMySqlEntity> query = _set.AsNoTracking();

            if (includeSubscriptions)
                query = query.Include(c => c.subscriptions);

            List<CustardMySqlEntity> list = await query.Where(predicate).ToListAsync();
            return Result.Success(list);
        }
        catch (Exception ex) { return Result.Failure<List<CustardMySqlEntity>>(ex.ToString()); }
    }

    public async Task<Result<CustardMySqlEntity>> GetByIdAsync(int id, bool includeSubscriptions = true)
    {
        IQueryable<CustardMySqlEntity> query = _set.AsNoTracking();

        if (includeSubscriptions)
            query = query.Include(c => c.subscriptions);

        CustardMySqlEntity? found = await query.SingleOrDefaultAsync(c => c.customerID == id);

        return found is null
            ? Result.Failure<CustardMySqlEntity>($"Entity with id {id} was not found")
            : Result.Success(found);
    }
}

