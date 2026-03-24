using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.MySql;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

namespace LeadPipe.Infrastructure.Data.DataSource;

public sealed class CaliperMySqlDataSource(
    ICaliperMySqlRepository repo,
    ISyncStateRepository sync
    ) : MySqlDataSource(sync), IDataSourceAsync<CaliperMySqlEntity>
{
    private readonly ICaliperMySqlRepository _repo = repo;
    private readonly Result<DateTimeOffset>? _latest;

    private async Task<Result<DateTimeOffset>> Latest() => _latest ?? await LatestSyncDate(SyncKey.Caliper);
    public async Task<Result<List<CaliperMySqlEntity>>> LoadAsync(bool withDetails)
    {
        // Retrieve all calipers since jan 1 last year
        DateTime dateFilter = new(DateTime.UtcNow.Year - 1, 1, 1);
        Result<List<CaliperMySqlEntity>> found = await _repo.FindAsync(c => c.called_at_utc >= dateFilter, withDetails);

        // Update latest sync date
        Result<DateTimeOffset> latest = await Latest();
        DateTimeOffset latestValue = latest.IsFailure
            ? DateTimeOffset.UtcNow.AddDays(-7) // This will only be used if no values are found in the db AND latest sync returns no date
            : latest.Value;
        DateTimeOffset updatedLatest = NewLatest(found, latestValue);
        await SyncStateAsync(updatedLatest, SyncKey.Caliper);

        return found;
    }

    public async Task<Result<List<CaliperMySqlEntity>>> RefreshAsync(bool withDetails)
    {
        // Find the most recent caliper date from syncstate
        Result<DateTimeOffset> latest = await Latest();
        if (latest.IsFailure)
            return await LoadAsync(withDetails);

        // Retrieve calipers on or after the latest date
        DateTime mostRecentDate = latest.Value.UtcDateTime.AddDays(-7);
        Result<List<CaliperMySqlEntity>> found = await _repo.FindAsync(c => c.called_at_utc >= mostRecentDate, withDetails);

        // Save new syncstate
        DateTimeOffset updatedLatest = NewLatest(found, mostRecentDate);

        await SyncStateAsync(updatedLatest, SyncKey.Caliper);

        return found;
    }

    private static DateTimeOffset NewLatest(Result<List<CaliperMySqlEntity>> found, DateTimeOffset defaultLatest)
        => found.IsSuccess && found.Value.Count > 0
            ? found.Value.Select(v => new DateTimeOffset(v.called_at_utc, TimeSpan.Zero)).Max()
            : defaultLatest;

}

public sealed class SyncedCaliperMySqlDataSource(
    ICaliperMySqlRepository repo,
    ISyncStateRepository sync,
    IClock clock
    ) : SyncedDataSourceBase<CaliperMySqlEntity>(sync, clock)
{
    private readonly ICaliperMySqlRepository _repo = repo;

    protected override SyncKey Key => SyncKey.Caliper;

    protected override DateTimeOffset GetLatest(Result<List<CaliperMySqlEntity>> entities)
        => entities.IsSuccess && entities.Value.Count > 0
            ? entities.Value.Select(v => new DateTimeOffset(v.called_at_utc, TimeSpan.Zero)).Max()
            : _clock.UtcNow.AddDays(-30);

    protected override async Task<Result<List<CaliperMySqlEntity>>> Load(bool withDetails)
    {
        DateTime dateFilter = new(_clock.UtcNow.Year - 1, 1, 1);
        Result<List<CaliperMySqlEntity>> loaded = await _repo.FindAsync(c => c.called_at_utc >= dateFilter, withDetails);
        return loaded;
    }

    protected override async Task<Result<List<CaliperMySqlEntity>>> Refresh(DateTimeOffset latest, bool withDetails)
    {
        DateTime mostRecentDate = latest.UtcDateTime.AddDays(-7);
        var found = await _repo.FindAsync(c => c.called_at_utc >= mostRecentDate, withDetails);
        return found;
    }
}