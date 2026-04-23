using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Repository;
using LeadPipe.Infrastructure.MySql.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LeadPipe.Infrastructure.MySql.Repository;

public class TranscriptionMySqlRepository(MySqlSchema2Context context) : ITranscriptionMySqlRepository
{
    private readonly DbSet<TranscriptionMySqlEntity> _set = context.Set<TranscriptionMySqlEntity>();

    public async Task<Result<List<TranscriptionMySqlEntity>>> FindAsync(Expression<Func<TranscriptionMySqlEntity, bool>> predicate)
    {
        try
        {
            List<TranscriptionMySqlEntity> list = await _set.AsNoTracking().Where(predicate).ToListAsync();
            return Result.Success(list);
        }
        catch (Exception ex) { return Result.Failure<List<TranscriptionMySqlEntity>>(ex.Message); }
    }

    public async Task<Result<TranscriptionMySqlEntity>> GetByIdAsync(long callId)
    {
        TranscriptionMySqlEntity? found = await _set.AsNoTracking().SingleOrDefaultAsync(t => t.call_id == callId);

        return found is null
            ? Result.Failure<TranscriptionMySqlEntity>($"{nameof(TranscriptionMySqlEntity)} with id {callId} was not found")
            : Result.Success(found);
    }
}
