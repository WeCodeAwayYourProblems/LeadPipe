using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;

namespace LeadPipe.Infrastructure.Repository;

public interface ICustomerCallMySqlRepository
{
    Task<Result<CustomerCallMySqlEntity>> AddAsync(CustomerCallMySqlEntity entity);
    Task<Result<List<CustomerCallMySqlEntity>>> AddRangeAsync(List<CustomerCallMySqlEntity> entities);
    Task<Result<bool>> DeleteAsync(CustomerCallMySqlEntity entity);
    Task<Result<bool>> DeleteAsync(long id);
    Task<Result<List<CustomerCallMySqlEntity>>> GetAllAsync();
    Task<Result<CustomerCallMySqlEntity>> GetByIdAsync(long id);
    Task<Result<CustomerCallMySqlEntity>> UpdateAsync(CustomerCallMySqlEntity entity);
}