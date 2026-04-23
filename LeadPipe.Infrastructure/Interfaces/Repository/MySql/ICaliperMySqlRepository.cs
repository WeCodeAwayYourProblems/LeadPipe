using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity;
using System.Linq.Expressions;

namespace LeadPipe.Infrastructure.Interfaces.Repository.MySql;

public interface ICaliperMySqlRepository
{
    Task<Result<List<CaliperMySqlEntity>>> FindAsync(Expression<Func<CaliperMySqlEntity, bool>> predicate, bool includeDetails = true);
    Task<Result<CaliperMySqlEntity>> GetByIdAsync(long id, bool includeDetails = true);
}
