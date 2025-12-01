using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;

namespace LeadPipe.Infrastructure.Repository;

public interface ICustardMySqlRepository
{
    Task<Result<CustardMySqlEntity>> AddAsync(CustardMySqlEntity entity);
    Task<Result<List<CustardMySqlEntity>>> AddRangeAsync(List<CustardMySqlEntity> entities);
    Task<Result<bool>> DeleteAsync(CustardMySqlEntity entity);
    Task<Result<bool>> DeleteAsync(long id);
    Task<Result<List<CustardMySqlEntity>>> GetAllAsync();
    Task<Result<CustardMySqlEntity>> GetByIdAsync(long id);
    Task<Result<CustardMySqlEntity>> UpdateAsync(CustardMySqlEntity entity);
}