using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces.Repository.MySql;
using LeadPipe.Infrastructure.MySql.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LeadPipe.Infrastructure.MySql.Repository;

public class SubMySqlRepository(MySqlSchemaContext context) : ISubMySqlRepository
{
    private readonly DbSet<SubMySqlEntity> _set = context.Set<SubMySqlEntity>();

    public async Task<Result<List<SubMySqlEntity>>> FindAsync(Expression<Func<SubMySqlEntity, bool>> predicate, bool includeCustomer = true)
    {
        try
        {
            IQueryable<SubMySqlEntity> query = _set.AsNoTracking();

            if (includeCustomer)
                query = query.Include(s => s.customer);

            List<SubMySqlEntity> list = await query.Where(predicate).ToListAsync();
            return Result.Success(list);
        }
        catch (Exception ex) { return Result.Failure<List<SubMySqlEntity>>(ex.Message); }
    }

    public async Task<Result<SubMySqlEntity>> GetByIdAsync(int id, bool includeCustomer = true)
    {
        IQueryable<SubMySqlEntity> query = _set.AsNoTracking();

        if (includeCustomer)
            query = query.Include(s => s.customer);

        SubMySqlEntity? found = await query.SingleOrDefaultAsync(s => s.subscriptionID == id);

        return found is null
            ? Result.Failure<SubMySqlEntity>($"{nameof(SubMySqlEntity)} with id {id} was not found")
            : Result.Success(found);
    }
}

