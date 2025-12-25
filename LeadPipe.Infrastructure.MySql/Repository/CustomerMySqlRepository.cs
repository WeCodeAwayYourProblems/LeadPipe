using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces.Repository.MySql;
using LeadPipe.Infrastructure.MySql.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LeadPipe.Infrastructure.MySql.Repository;

public class CustomerMySqlRepository(MySqlSchemaContext context) : ICustomerMySqlRepository
{
    private readonly DbSet<CustomerMySqlEntity> _set = context.Set<CustomerMySqlEntity>();

    public async Task<Result<List<CustomerMySqlEntity>>> FindAsync(Expression<Func<CustomerMySqlEntity, bool>> predicate, bool includeSubscriptions = true)
    {
        try
        {
            IQueryable<CustomerMySqlEntity> query = _set.AsNoTracking();

            if (includeSubscriptions)
                query = query.Include(c => c.subscriptions);

            List<CustomerMySqlEntity> list = await query.Where(predicate).ToListAsync();
            return Result.Success(list);
        }
        catch (Exception ex) { return Result.Failure<List<CustomerMySqlEntity>>(ex.Message); }
    }

    public async Task<Result<CustomerMySqlEntity>> GetByIdAsync(int id, bool includeSubscriptions = true)
    {
        IQueryable<CustomerMySqlEntity> query = _set.AsNoTracking();

        if (includeSubscriptions)
            query = query.Include(c => c.subscriptions);

        CustomerMySqlEntity? found = await query.SingleOrDefaultAsync(c => c.customerID == id);

        return found is null
            ? Result.Failure<CustomerMySqlEntity>($"Entity with id {id} was not found")
            : Result.Success(found);
    }
}

