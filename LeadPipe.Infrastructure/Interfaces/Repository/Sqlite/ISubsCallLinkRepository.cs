using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;

namespace LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

public interface ISubsCallLinkRepository : IRepository<CallSubsLink>
{
    Task<Result<List<CallSubsLink>>> GetAllWithDetailsAsync();
}
