using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces.Repository.MySql;
using LeadPipe.Infrastructure.MySql.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LeadPipe.Infrastructure.MySql.Repository;

public class CallMySqlRepository(MySqlSchema2Context context) : ICallMySqlRepository
{
    private readonly DbSet<CallMySqlEntity> _set = context.Set<CallMySqlEntity>();

    public async Task<Result<List<CallMySqlEntity>>> FindAsync(Expression<Func<CallMySqlEntity, bool>> predicate, bool includeDetails = true)
    {
        try
        {
            IQueryable<CallMySqlEntity> query = _set.AsNoTracking();

            if (includeDetails)
            {
                query = query
                    .Include(c => c.summaries)
                    .Include(c => c.transcriptions);

                // If this query ever takes a long time, this will be helpful
                //query = _set
                //    .AsNoTracking()
                //    .AsSplitQuery() // If there are a lot of calls and many summaries and transcriptions for each call
                //    .Include(c => c.summaries)
                //    .Include(c => c.transcriptions);
            }

            List<CallMySqlEntity> list = await query.Where(predicate).ToListAsync();
            return Result.Success(list);
        }
        catch (Exception ex) { return Result.Failure<List<CallMySqlEntity>>(ex.Message); }
    }

    public async Task<Result<CallMySqlEntity>> GetByIdAsync(long id, bool includeDetails = true)
    {
        IQueryable<CallMySqlEntity> query = _set.AsNoTracking();

        if (includeDetails)
        {
            query = query
                .Include(c => c.summaries)
                .Include(c => c.transcriptions);
        }

        CallMySqlEntity? found = await query.SingleOrDefaultAsync(c => c.call_id == id);

        return found is null
            ? Result.Failure<CallMySqlEntity>($"{nameof(CallMySqlEntity)} with id {id} was not found")
            : Result.Success(found);
    }
}
