using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;

namespace LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

public interface ISubsCallLinkRepository : IRepository<SubsCallLink>
{
    Task<Result<List<SubsCallLink>>> GetAllWithDetailsAsync();
    Task<Result<List<SubsCallLink>>> GetAllWithDetailsAsync(List<CallEntity> list);
}
