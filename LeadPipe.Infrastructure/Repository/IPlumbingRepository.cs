using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity;

namespace LeadPipe.Infrastructure.Repository;

public interface IPlumbingRepository
{
    Task<Result> AddAsync(PlumbingEntity entity);
    Task<Result> AddRangeAsync(List<PlumbingEntity> entities);
    Task<Result> DeleteAsync(long id);
    Task<Result> DeleteAsync(PlumbingEntity entity);
    Task<Result<List<PlumbingEntity>>> GetAllAsync();
    Task<Result<PlumbingEntity>> GetAsync(PlumbingEntity entity);
    Task<Result<PlumbingEntity>> GetByIdAsync(long id);
    Task<Result> HardUpdateAsync(PlumbingEntity entity);
    Task<Result> UpdateValuesAsync(PlumbingEntity entity);
}