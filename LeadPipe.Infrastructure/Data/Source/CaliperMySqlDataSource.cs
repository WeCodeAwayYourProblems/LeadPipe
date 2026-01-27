using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.MySql;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

namespace LeadPipe.Infrastructure.Data.Source;

public sealed class CaliperMySqlDataSource(
    ICaliperMySqlRepository repo,
    IRepository<CaliperEntity> caliper
    ) : IDataSourceAsync<CaliperMySqlEntity>
{
    private readonly ICaliperMySqlRepository _repo = repo;
    private readonly IRepository<CaliperEntity> _caliper = caliper;
    public async Task<Result<List<CaliperMySqlEntity>>> LoadAsync()
    {
        DateTime dateFilter = DateTime.UtcNow.AddYears(-1);
        Result<List<CaliperMySqlEntity>> found = await _repo.FindAsync(c => c.called_at_utc >= dateFilter, true);
        return found;
    }

    public async Task<Result<List<CaliperMySqlEntity>>> RefreshAsync()
    {
        // Find the most recent caliper date in the caliper repository
        Result<List<CaliperEntity>> calipersResult = await _caliper.GetAllAsync();
        if (calipersResult.IsFailure || calipersResult.Value.Count == 0)
            return await LoadAsync();

        DateOnly mostRecent = DateOnly.FromDateTime(calipersResult.Value.Max(m => m.Date));
        DateTime mostRecentDate = new DateTime(mostRecent, new TimeOnly(0)) - TimeSpan.FromDays(7); 
        Result<List<CaliperMySqlEntity>> found = await _repo.FindAsync(c => c.called_at_utc >= mostRecentDate, includeDetails: true);
        return found;
    }
}
