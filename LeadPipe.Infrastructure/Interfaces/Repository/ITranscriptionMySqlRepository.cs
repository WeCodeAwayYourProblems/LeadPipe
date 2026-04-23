using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity;
using System.Linq.Expressions;

namespace LeadPipe.Infrastructure.Interfaces.Repository;

public interface ITranscriptionMySqlRepository
{
    Task<Result<List<TranscriptionMySqlEntity>>> FindAsync(Expression<Func<TranscriptionMySqlEntity, bool>> predicate);
    Task<Result<TranscriptionMySqlEntity>> GetByIdAsync(long callId);
}