using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;

namespace LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

public interface ISubsPlumbingLinkRepository : IRepository<SubsPlumbingLink>
{
    public Task<Result<List<SubsPlumbingLink>>> GetAllWithDetailsAsync();
    public Task<Result<List<SubsPlumbingLink>>> GetAllWithDetailsAsync(IEnumerable<PlumbingEntity> filter);
    public Task<Result<List<SubsPlumbingLink>>> GetAllAsync(IEnumerable<PlumbingEntity> filter);
}
