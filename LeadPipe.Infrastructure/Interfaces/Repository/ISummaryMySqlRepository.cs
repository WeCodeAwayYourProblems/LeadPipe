using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;

namespace LeadPipe.Infrastructure.Interfaces.Repository;

public interface ISummaryMySqlRepository
{
    Task<Result<SummaryMySqlEntity>> AddAsync(SummaryMySqlEntity entity);
    Task<Result<List<SummaryMySqlEntity>>> AddRangeAsync(List<SummaryMySqlEntity> entities);
    Task<Result<bool>> DeleteAsync(long id);
    Task<Result<bool>> DeleteAsync(SummaryMySqlEntity entity);
    Task<Result<List<SummaryMySqlEntity>>> GetAllAsync();
    Task<Result<SummaryMySqlEntity>> GetByIdAsync(long id);
    Task<Result<SummaryMySqlEntity>> UpdateAsync(SummaryMySqlEntity entity);
}