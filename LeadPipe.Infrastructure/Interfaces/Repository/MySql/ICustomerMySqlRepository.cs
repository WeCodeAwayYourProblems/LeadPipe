using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;

namespace LeadPipe.Infrastructure.Interfaces.Repository.MySql;

public interface ICustomerMySqlRepository
{
    Task<Result<CustomerMySqlEntity>> AddAsync(CustomerMySqlEntity entity);
    Task<Result<List<CustomerMySqlEntity>>> AddRangeAsync(List<CustomerMySqlEntity> entities);
    Task<Result<bool>> DeleteAsync(CustomerMySqlEntity entity);
    Task<Result<bool>> DeleteAsync(long id);
    Task<Result<List<CustomerMySqlEntity>>> GetAllAsync();
    Task<Result<CustomerMySqlEntity>> GetByIdAsync(long id);
    Task<Result<CustomerMySqlEntity>> UpdateAsync(CustomerMySqlEntity entity);
}
