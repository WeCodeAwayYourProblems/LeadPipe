using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;

namespace LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

public interface IPlumbingCallLinkRepository : IRepository<PlumbingCallLink>
{
    Task<Result<List<PlumbingCallLink>>> GetAllWithDetailsAsync();
}
