using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;

namespace LeadPipe.Infrastructure.Interfaces.Repository;

public interface ICallMySqlRepository
{
    Task<Result<CallMySqlEntity>> AddAsync(CallMySqlEntity entity);
    Task<Result<List<CallMySqlEntity>>> AddRangeAsync(List<CallMySqlEntity> entities);
    Task<Result<bool>> DeleteAsync(CallMySqlEntity entity);
    Task<Result<bool>> DeleteAsync(long id);
    Task<Result<List<CallMySqlEntity>>> GetAllAsync();
    Task<Result<CallMySqlEntity>> GetByIdAsync(long id);
    Task<Result<CallMySqlEntity>> UpdateAsync(CallMySqlEntity entity);
}