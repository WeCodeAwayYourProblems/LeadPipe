using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity;

namespace LeadPipe.Infrastructure.Repository;

internal interface ISubsRepository
{
    Task<Result> AddAsync(SubsEntity entity);
    Task<Result> AddRangeAsync(List<SubsEntity> entities);
    Task<Result> DeleteAsync(long phoneNumber);
    Task<Result> DeleteAsync(SubsEntity entity);
    Task<Result<List<SubsEntity>>> GetAllAsync();
    Task<Result<SubsEntity>> GetAsync(SubsEntity entity);
    Task<Result<SubsEntity>> GetByIdAsync(long id);
    Task<Result> HardUpdateAsync(SubsEntity entity);
    Task<Result> UpdateValuesAsync(SubsEntity entity);
}