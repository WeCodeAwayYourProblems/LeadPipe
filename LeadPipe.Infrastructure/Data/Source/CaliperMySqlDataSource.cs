using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.MySql;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

namespace LeadPipe.Infrastructure.Data.Source;

public sealed class CaliperMySqlDataSource(
    ICaliperMySqlRepository repo,
    ICaliperRepository calls
    ) : IDataSourceAsync<CaliperMySqlEntity>
{
    private readonly ICaliperMySqlRepository _repo = repo;
    private readonly ICaliperRepository _calls = calls;
    public async Task<Result<List<CaliperMySqlEntity>>> LoadAsync()
    {
        DateTime dateFilter = DateTime.UtcNow.AddYears(-1);
        Result<List<CaliperMySqlEntity>> found = await _repo.FindAsync(c => c.called_at_utc >= dateFilter, true);
        return found;
    }

    public async Task<Result<List<CaliperMySqlEntity>>> RefreshAsync()
    {
        // Find the most recent call date in the call repository
        Result<List<CaliperEntity>> callsResult = await _calls.GetAllAsync();
        if (callsResult.IsFailure || callsResult.Value.Count == 0)
            return await LoadAsync();

        DateOnly mostRecent = DateOnly.FromDateTime(callsResult.Value.Max(m => m.CaliperDate));
        DateTime mostRecentDate = new DateTime(mostRecent, new TimeOnly(0)) - TimeSpan.FromDays(7); 
        Result<List<CaliperMySqlEntity>> found = await _repo.FindAsync(c => c.called_at_utc >= mostRecentDate, includeDetails: true);
        return found;
    }
}
