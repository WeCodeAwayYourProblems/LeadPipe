using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.MySql;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

namespace LeadPipe.Infrastructure.Data.DataSource;

public sealed class CustardMySqlDataSource(
    ICustardMySqlRepository repo,
    ISyncStateRepository sync
) : MySqlDataSource(sync), IDataSourceAsync<CustardMySqlEntity>
{
    private readonly ICustardMySqlRepository _repo = repo;
    public async Task<Result<List<CustardMySqlEntity>>> LoadAsync(bool withDetails)
    {
        DateTime twentyTwelve = new(new DateOnly(2012, 1, 1), new TimeOnly(0), DateTimeKind.Utc);
        Result<List<CustardMySqlEntity>> found = await _repo.FindAsync(s => s.dateAdded >= twentyTwelve, withDetails);

        DateTimeOffset latest = Latest(found);
        await SyncStateAsync(latest, SyncKey.Custard);
        return found;
    }

    public async Task<Result<List<CustardMySqlEntity>>> RefreshAsync(bool withDetails)
    {
        // Retrieve data from sync
        Result<DateTimeOffset> syncDate = await LatestSyncDate(SyncKey.Custard);
        if (syncDate.IsFailure)
            return await LoadAsync(withDetails);

        Result<List<CustardMySqlEntity>> found = await _repo.FindAsync(s => s.dateAdded >= syncDate.Value, withDetails);

        DateTimeOffset latest = Latest(found);
        await SyncStateAsync(latest, SyncKey.Custard);

        return found;
    }

    private static DateTimeOffset Latest(Result<List<CustardMySqlEntity>> found) =>
        found.IsSuccess && found.Value.Count > 0
            ? new(found.Value.Max(v => v.dateAdded) ?? DateTime.UtcNow.AddDays(-30), TimeSpan.Zero)
            : DateTimeOffset.UtcNow.AddDays(-30);
}

public sealed class CustardMySqlDataSourceBased(
    ICustardMySqlRepository repo,
    ISyncStateRepository sync,
    IClock clock
) : SyncedDataSourceBase<CustardMySqlEntity>(sync, clock)
{
    private readonly ICustardMySqlRepository _repo = repo;
    protected override SyncKey Key => SyncKey.Custard;

    protected override DateTimeOffset GetLatest(Result<List<CustardMySqlEntity>> entities)
        => entities.IsSuccess && entities.Value.Count > 0
            ? new(entities.Value.Max(v => v.dateAdded) ?? _clock.UtcNow.UtcDateTime.AddDays(-30), TimeSpan.Zero)
            : _clock.UtcNow.AddDays(-30);

    protected override async Task<Result<List<CustardMySqlEntity>>> Load(bool withDetails)
    {
        DateTime twentyTwelve = new(2012, 1, 1, 0, 0, 0);
        Result<List<CustardMySqlEntity>> found = await _repo.FindAsync(s => s.dateAdded >= twentyTwelve, withDetails);
        return found;
    }

    protected override async Task<Result<List<CustardMySqlEntity>>> Refresh(DateTimeOffset latest, bool withDetails)
    {
        DateTime syncDate = latest.UtcDateTime.AddDays(-7);
        Result<List<CustardMySqlEntity>> found = await _repo.FindAsync(s => s.dateAdded >= syncDate, withDetails);
        return found;
    }
}