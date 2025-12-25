using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.MySql;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

namespace LeadPipe.Infrastructure.Data.Source;

public sealed class CallMySqlDataSource(
    ICallMySqlRepository repo,
    ICallRepository calls
    ) : IDataSourceAsync<CallMySqlEntity>
{
    private readonly ICallMySqlRepository _repo = repo;
    private readonly ICallRepository _calls = calls;
    public async Task<Result<List<CallMySqlEntity>>> LoadAsync()
    {
        DateTime dateFilter = DateTime.UtcNow.AddYears(-2);
        Result<List<CallMySqlEntity>> found = await _repo.FindAsync(c => c.called_at_utc >= dateFilter, true);
        return found;
    }

    public async Task<Result<List<CallMySqlEntity>>> RefreshAsync()
    {
        // Find the most recent call date in the call repository
        Result<List<CallEntity>> callsResult = await _calls.GetAllAsync();
        if (callsResult.IsFailure || callsResult.Value.Count == 0)
            return await LoadAsync();

        DateOnly mostRecent = DateOnly.FromDateTime(callsResult.Value.Max(m => m.CallDate));
        DateTime mostRecentDate = new(mostRecent, new TimeOnly(0)); 
        Result<List<CallMySqlEntity>> found = await _repo.FindAsync(c => c.called_at_utc >= mostRecentDate, includeDetails: true);
        return found;
    }
}
