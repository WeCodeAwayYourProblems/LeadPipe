using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces.Repository.MySql;
using LeadPipe.Infrastructure.MySql.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LeadPipe.Infrastructure.MySql.Repository;

public class SummaryMySqlRepository(MySqlContext context) : ISummaryMySqlRepository
{
    private readonly DbSet<SummaryMySqlEntity> _set = context.Set<SummaryMySqlEntity>();

    public async Task<Result<List<SummaryMySqlEntity>>> FindAsync(Expression<Func<SummaryMySqlEntity, bool>> predicate)
    {
        try
        {
            List<SummaryMySqlEntity> list = await _set
                .AsNoTracking()
                .Where(predicate)
                .ToListAsync();

            return Result.Success(list);
        }
        catch (Exception ex) { return Result.Failure<List<SummaryMySqlEntity>>(ex.Message); }
    }

    public async Task<Result<SummaryMySqlEntity>> GetByIdAsync(long callId)
    {
        SummaryMySqlEntity? found = await _set
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.call_id == callId);

        return found is null
            ? Result.Failure<SummaryMySqlEntity>($"{nameof(SummaryMySqlEntity)} with id {callId} was not found")
            : Result.Success(found);
    }
}
