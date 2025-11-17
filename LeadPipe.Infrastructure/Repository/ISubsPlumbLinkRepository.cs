using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity;

namespace LeadPipe.Infrastructure.Repository
{
    internal interface ISubsPlumbLinkRepository
    {
        Task<Result> AddAsync(SubsPlumbingLink entity);
        Task<Result> AddRangeAsync(List<SubsPlumbingLink> entities);
        Task<Result> DeleteAsync(long subsId, long plumbId);
        Task<Result> DeleteAsync(SubsPlumbingLink entity);
        Task<Result<List<SubsPlumbingLink>>> GetAllAsync();
        Task<Result<SubsPlumbingLink>> GetAsync(SubsPlumbingLink entity);
        Task<Result<SubsPlumbingLink>> GetByIdAsync(long subsId, long plumbId);
        Task<Result> HardUpdateAsync(SubsPlumbingLink entity);
        Task<Result> UpdateValuesAsync(SubsPlumbingLink entity);
    }
}