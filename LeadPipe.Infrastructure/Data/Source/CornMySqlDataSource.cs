using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.MySql;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

namespace LeadPipe.Infrastructure.Data.Source;

public sealed class CornMySqlDataSource(
    ICornMySqlRepository repo,
    IRepositoryFactory factory
) : IDataSourceAsync<CornMySqlEntity>
{
    private readonly ICornMySqlRepository _repo = repo;
    private readonly IRepository<CornEntity> _corn = factory.GetRepository<CornEntity>();

    public async Task<Result<List<CornMySqlEntity>>> LoadAsync(bool _ = false)
    {
        DateTime twentyTwelve = new(new DateOnly(2012, 1, 1), new TimeOnly(0), DateTimeKind.Utc);
        Result<List<CornMySqlEntity>> found = await _repo.FindAsync(s => s.timestamp >= twentyTwelve);
        return found;
    }

    public async Task<Result<List<CornMySqlEntity>>> RefreshAsync(bool _ = false)
    {
        // Find the most recent corn date in the corn repo
        Result<List<CornEntity>> existing = await _corn.GetAllAsync();
        if (existing.IsFailure || existing.Value.Count == 0)
            return await LoadAsync();

        DateOnly mostRecent = DateOnly.FromDateTime(existing.Value.Max(m => m.Date));
        DateTime mostRecentDate = new DateTime(mostRecent, new TimeOnly(0)) - TimeSpan.FromDays(-7);
        var result = await _repo.FindAsync(c => c.timestamp >= mostRecentDate);
        return result;
    }
}