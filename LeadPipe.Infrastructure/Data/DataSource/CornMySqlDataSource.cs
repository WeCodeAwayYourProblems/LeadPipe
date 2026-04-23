using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository;

namespace LeadPipe.Infrastructure.Data.DataSource;

public sealed class CornMySqlDataSource(
    ICornMySqlRepository repo,
    ISyncStateRepository sync
) : MySqlDataSource(sync), IDataSourceAsync<CornMySqlEntity>
{
    private readonly ICornMySqlRepository _repo = repo;

    public async Task<Result<List<CornMySqlEntity>>> LoadAsync(bool _ = false)
    {
        DateTime twentyTwelve = new(new DateOnly(2012, 1, 1), new TimeOnly(0), DateTimeKind.Utc);
        Result<List<CornMySqlEntity>> found = await _repo.FindAsync(s => s.timestamp >= twentyTwelve);

        await SaveNewState(found);

        return found;
    }

    public async Task<Result<List<CornMySqlEntity>>> RefreshAsync(bool _ = false)
    {
        // find the most recent sync with corn
        Result<DateTimeOffset> latest = await LatestSyncDate(SyncKey.CornFormula);
        if (latest.IsFailure)
            return await LoadAsync();

        // Get latest date
        var mostRecentDate = latest.Value.UtcDateTime.AddDays(-7);
        Result<List<CornMySqlEntity>> result = await _repo.FindAsync(c => c.timestamp >= mostRecentDate);

        // Expose result
        await SaveNewState(result);

        return result;
    }

    private async Task SaveNewState(Result<List<CornMySqlEntity>> result)
    {
        DateTimeOffset newLatest = result.IsSuccess && result.Value.Count > 0
            ? new DateTimeOffset(result.Value.Max(v => v.timestamp), TimeSpan.Zero)
            : DateTime.UtcNow.AddDays(-30);
        await SyncStateAsync(newLatest, SyncKey.CornFormula);
    }
}

public sealed class CornMySqlDataSourceBased(
    ICornMySqlRepository repo,
    ISyncStateRepository sync,
    IClock clock
) : SyncedDataSourceBase<CornMySqlEntity>(sync, clock)
{
    private readonly ICornMySqlRepository _repo = repo;
    protected override SyncKey Key => SyncKey.CornFormula;

    protected override DateTimeOffset GetLatest(Result<List<CornMySqlEntity>> entities)
        => entities.IsSuccess && entities.Value.Count > 0
            ? new DateTimeOffset(entities.Value.Max(v => v.timestamp), TimeSpan.Zero)
            : _clock.UtcNow.AddDays(-30);

    protected override async Task<Result<List<CornMySqlEntity>>> Load(bool _ = default)
    {
        DateTime twentyTwelve = new(2012, 1, 1);
        Result<List<CornMySqlEntity>> found = await _repo.FindAsync(s => s.timestamp >= twentyTwelve);
        return found;
    }

    protected override async Task<Result<List<CornMySqlEntity>>> Refresh(DateTimeOffset latest, bool withDetails)
    {
        DateTime mostRecentDate = latest.UtcDateTime.AddDays(-7);
        Result<List<CornMySqlEntity>> result = await _repo.FindAsync(c => c.timestamp >= mostRecentDate);
        return result;
    }
}