using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Repository.MySql;
using LeadPipe.Infrastructure.MySql.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LeadPipe.Infrastructure.MySql.Repository;

public class SandMySqlRepository(MySqlSchema1Context context) : ISandMySqlRepository
{
    private readonly DbSet<SandMySqlEntity> _set = context.Set<SandMySqlEntity>();

    public async Task<Result<List<SandMySqlEntity>>> FindAsync(Expression<Func<SandMySqlEntity, bool>> predicate, bool includeDetails = true)
    {
        try
        {
            IQueryable<SandMySqlEntity> query = _set.AsNoTracking();

            if (includeDetails)
                query = query
                    .Include(s => s.customer)
                    .Include(s => s.offerman);

            List<SandMySqlEntity> list = await query.Where(predicate).ToListAsync();
            return Result.Success(list);
        }
        catch (Exception ex) { return Result.Failure<List<SandMySqlEntity>>(ex.ToString()); }
    }

    public async Task<Result<SandMySqlEntity>> GetByIdAsync(int id, bool includeDetails = true)
    {
        IQueryable<SandMySqlEntity> query = _set.AsNoTracking();

        if (includeDetails)
            query = query
                .Include(s => s.customer)
                .Include(s => s.offerman);

        SandMySqlEntity? found = await query.SingleOrDefaultAsync(s => s.subscriptionID == id);

        return found is null
            ? Result.Failure<SandMySqlEntity>($"{nameof(SandMySqlEntity)} with id {id} was not found")
            : Result.Success(found);
    }
}

