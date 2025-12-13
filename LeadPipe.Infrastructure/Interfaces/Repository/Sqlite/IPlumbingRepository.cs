using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;

namespace LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

public interface IPlumbingRepository : IRepository<PlumbingEntity>
{
    public Task<Result<List<PlumbingEntity>>> GetAllAsync(Source source);
}
