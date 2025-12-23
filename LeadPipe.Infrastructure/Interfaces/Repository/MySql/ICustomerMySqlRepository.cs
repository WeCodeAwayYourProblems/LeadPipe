using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using System.Linq.Expressions;

namespace LeadPipe.Infrastructure.Interfaces.Repository.MySql;

public interface ICustomerMySqlRepository
{
    Task<Result<List<CustomerMySqlEntity>>> FindAsync(Expression<Func<CustomerMySqlEntity, bool>> predicate, bool includeSubscriptions = true);
    Task<Result<CustomerMySqlEntity>> GetByIdAsync(int id, bool includeSubscriptions = true);
}
