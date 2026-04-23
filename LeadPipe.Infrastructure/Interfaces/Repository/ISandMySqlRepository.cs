using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity;
using System.Linq.Expressions;

namespace LeadPipe.Infrastructure.Interfaces.Repository;

public interface ISandMySqlRepository
{
    Task<Result<List<SandMySqlEntity>>> FindAsync(Expression<Func<SandMySqlEntity, bool>> predicate, bool includeCustomer = true);
    Task<Result<SandMySqlEntity>> GetByIdAsync(int id, bool includeCustomer = true);
}
