using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Service;

namespace LeadPipe.Infrastructure.Data.DataSource;

public class CatManDataSource(ICatManService cat, ISyncStateRepository state) : IDataSourceAsync<CatManDto>
{
    private readonly ICatManService _cat = cat;
    private readonly ISyncStateRepository _state = state;
    private readonly DateTime Today = DateTime.UtcNow;
    private readonly DateTimeOffset Now = DateTimeOffset.UtcNow;
    private readonly SyncKey Key = SyncKey.Catman;
    public async Task<Result<List<CatManDto>>> LoadAsync(bool _ = default)
    {
        DateTime twentyTwelve = new(2025, 1, 1);
        Result<List<CatManDto>> get = await _cat.GetAllAsync(twentyTwelve, Today);

        DateTimeOffset syncDate = GetDate(get);
        await SyncStateAsync(_state, syncDate, Key);
        return get;
    }

    public async Task<Result<List<CatManDto>>> RefreshAsync(bool _ = default)
    {
        // Get most recent refresh date
        Result<SyncStateEntity> state = await _state.GetByKeyAsync(null, Key);
        if (state.IsFailure)
            return await LoadAsync();

        DateTime lastSync = DateTimeOffset.FromUnixTimeSeconds(state.Value.UnixLastSyncUtc).UtcDateTime;
        Result<List<CatManDto>> result = await _cat.GetAllAsync(lastSync, Today);

        DateTimeOffset syncDate = GetDate(result);
        await SyncStateAsync(_state, syncDate, Key);
        return result;
    }
    internal DateTimeOffset GetDate(Result<List<CatManDto>> get)
    {
        return get.IsSuccess
            ? DateTimeOffset.FromUnixTimeSeconds(get.Value.Min(v => v.unix_time) ?? Now.ToUnixTimeSeconds())
            : Now;
    }
    private static async Task<Result> SyncStateAsync(ISyncStateRepository state, DateTimeOffset date, SyncKey key)
    {
        SyncStateEntity catmanstate = new()
        {
            BusinessId = BusinessId.From(key.Value),
            LastSyncUtc = date.UtcDateTime,
            UnixLastSyncUtc = date.ToUnixTimeSeconds()
        };
        var upsert = await state.UpsertRangeAsync([catmanstate]);
        return upsert;
    }
}

public sealed class CatManDataSourceBased(
    ICatManService cat,
    ISyncStateRepository sync,
    IClock clock
) : SyncedDataSourceBase<CatManDto>(sync, clock)
{
    private readonly ICatManService _cat = cat;
    private long UnixNow => _clock.UtcNow.ToUnixTimeSeconds();
    protected override SyncKey Key => SyncKey.Catman;

    protected override DateTimeOffset GetLatest(Result<List<CatManDto>> entities)
    {
        long unixLatest = entities.IsSuccess && entities.Value.Count > 0
            ? entities.Value.Max(v => v.unix_time) ?? UnixNow
            : UnixNow;
        var latest = DateTimeOffset.FromUnixTimeSeconds(unixLatest);
        return latest;
    }

    protected override async Task<Result<List<CatManDto>>> Load(bool withDetails)
    {
        DateTime twentyTwelve = new(2025, 1, 1);
        DateTime today = _clock.UtcNow.UtcDateTime;
        Result<List<CatManDto>> found = await _cat.GetAllAsync(twentyTwelve, today);
        return found;
    }

    protected override async Task<Result<List<CatManDto>>> Refresh(DateTimeOffset latest, bool _ = default)
    {
        DateTime lastSync = latest.UtcDateTime;
        DateTime today = _clock.UtcNow.UtcDateTime;
        Result<List<CatManDto>> result = await _cat.GetAllAsync(lastSync, today);
        return result;
    }
}