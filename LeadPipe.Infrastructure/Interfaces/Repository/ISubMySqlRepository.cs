using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;

namespace LeadPipe.Infrastructure.Interfaces.Repository;

public interface ISubMySqlRepository
{
    Task<Result<SubMySqlEntity>> AddAsync(SubMySqlEntity entity);
    Task<Result<List<SubMySqlEntity>>> AddRangeAsync(List<SubMySqlEntity> entities);
    Task<Result<bool>> DeleteAsync(long id);
    Task<Result<bool>> DeleteAsync(SubMySqlEntity entity);
    Task<Result<List<SubMySqlEntity>>> GetAllAsync();
    Task<Result<SubMySqlEntity>> GetByIdAsync(long id);
    Task<Result<SubMySqlEntity>> UpdateAsync(SubMySqlEntity entity);
}