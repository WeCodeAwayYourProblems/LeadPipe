using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces.Repository.MySql;
using LeadPipe.Infrastructure.MySql.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LeadPipe.Infrastructure.MySql.Repository;

public class CaliperMySqlRepository(MySqlSchema2Context context) : ICaliperMySqlRepository
{
    private readonly DbSet<CaliperMySqlEntity> _set = context.Set<CaliperMySqlEntity>();

    public async Task<Result<List<CaliperMySqlEntity>>> FindAsync(Expression<Func<CaliperMySqlEntity, bool>> predicate, bool includeDetails = true)
    {
        try
        {
            IQueryable<CaliperMySqlEntity> query = _set.AsNoTracking();

            if (includeDetails)
            {
                query = query
                    .Include(c => c.summaries)
                    .Include(c => c.transcriptions);
            }

            List<CaliperMySqlEntity> list = await query.Where(predicate).ToListAsync();
            return Result.Success(list);
        }
        catch (Exception ex) { return Result.Failure<List<CaliperMySqlEntity>>(ex.ToString()); }
    }

    public async Task<Result<CaliperMySqlEntity>> GetByIdAsync(long id, bool includeDetails = true)
    {
        IQueryable<CaliperMySqlEntity> query = _set.AsNoTracking();

        if (includeDetails)
        {
            query = query
                .Include(c => c.summaries)
                .Include(c => c.transcriptions);
        }

        CaliperMySqlEntity? found = await query.SingleOrDefaultAsync(c => c.call_id == id);

        return found is null
            ? Result.Failure<CaliperMySqlEntity>($"{nameof(CaliperMySqlEntity)} with id {id} was not found")
            : Result.Success(found);
    }
}
