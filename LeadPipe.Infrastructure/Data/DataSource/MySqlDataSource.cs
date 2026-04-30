using CSharpFunctionalExtensions;
using LeadPipe.Core;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository;

namespace LeadPipe.Infrastructure.Data.DataSource;

public class MySqlDataSource(ISyncStateRepository sync)
{
    private readonly ISyncStateRepository _sync = sync;
    protected async Task<Result> SyncStateAsync(DateTimeOffset dateUpdated, SyncKey key)
    {
        SyncStateEntity state = new()
        {
            BusinessId = BusinessId.From(key.Value),
            LastSyncUtc = dateUpdated.UtcDateTime,
            UnixLastSyncUtc = dateUpdated.ToUnixTime()
        };

        Result<List<SyncStateEntity>> upsert = await _sync.UpsertRangeAsync([state]);

        return upsert;
    }

    protected async Task<Result<DateTimeOffset>> LatestSyncDate(SyncKey key)
    {
        Result<SyncStateEntity> state = await _sync.GetByKeyAsync(null, key);
        if (state.IsFailure)
            return Result.Failure<DateTimeOffset>(state.Error);

        DateTimeOffset syncDate = DateTimeOffsetExt.FromUnixTime(state.Value.UnixLastSyncUtc);

        return syncDate;
    }

}
public abstract class SyncedDataSourceBase<TEntity>(
    ISyncStateRepository sync,
    IClock clock
) : IDataSourceAsync<TEntity>
{
    private readonly ISyncStateRepository _sync = sync;
    protected readonly IClock _clock = clock;
    protected async Task<Result> SyncStateAsync(DateTimeOffset dateUpdated, SyncKey key)
    {
        SyncStateEntity state = new()
        {
            BusinessId = BusinessId.BuildBusinessId(null, key),
            LastSyncUtc = dateUpdated.UtcDateTime,
            UnixLastSyncUtc = dateUpdated.ToUnixTime()
        };

        Result<List<SyncStateEntity>> upsert = await _sync.UpsertRangeAsync([state]);

        return upsert;
    }

    protected async Task<Result<DateTimeOffset>> LatestSyncDate(SyncKey key)
    {
        Result<SyncStateEntity> state = await _sync.GetByKeyAsync(null, key);
        if (state.IsFailure)
            return Result.Failure<DateTimeOffset>(state.Error);

        DateTimeOffset syncDate = DateTimeOffsetExt.FromUnixTime(state.Value.UnixLastSyncUtc);

        return syncDate;
    }
    protected abstract Task<Result<List<TEntity>>> Load(bool withDetails);
    protected abstract Task<Result<List<TEntity>>> Refresh(DateTimeOffset latest, bool withDetails);
    protected abstract SyncKey Key { get; }
    protected abstract DateTimeOffset GetLatest(Result<List<TEntity>> entities);
    public async Task<Result<List<TEntity>>> LoadAsync(bool withDetails)
    {
        Result<List<TEntity>> loaded = await Load(withDetails);

        DateTimeOffset newLatest = GetLatest(loaded);
        await SyncStateAsync(newLatest, Key);

        return loaded;
    }

    public async Task<Result<List<TEntity>>> RefreshAsync(bool withDetails)
    {
        Result<DateTimeOffset> latest = await LatestSyncDate(Key);
        if (latest.IsFailure)
            return await LoadAsync(withDetails);

        Result<List<TEntity>> refreshed = await Refresh(latest.Value, withDetails);

        DateTimeOffset newLatest = GetLatest(refreshed);
        await SyncStateAsync(newLatest, Key);

        return refreshed;
    }
}