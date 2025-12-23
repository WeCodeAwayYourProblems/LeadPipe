using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using System.Linq.Expressions;

namespace LeadPipe.Infrastructure.Interfaces.Repository.MySql;

public interface ICallMySqlRepository
{
    Task<Result<List<CallMySqlEntity>>> FindAsync(Expression<Func<CallMySqlEntity, bool>> predicate, bool includeDetails = true);
    Task<Result<CallMySqlEntity>> GetByIdAsync(long id, bool includeDetails = true);
}
