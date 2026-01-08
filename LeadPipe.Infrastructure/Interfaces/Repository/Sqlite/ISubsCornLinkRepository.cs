using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;

namespace LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

public interface ISubsCornLinkRepository : IRepository<SubsCornLink>
{
    Task<Result<List<SubsCornLink>>> GetAllWithDetailsAsync();
    Task<Result<List<SubsCornLink>>> GetAllWithDetailsAsync(IEnumerable<CornEntity> filter);
    Task<Result<List<SubsCornLink>>> GetAllAsync(IEnumerable<CornEntity> filter);
}