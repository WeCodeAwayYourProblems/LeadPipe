using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Service;

namespace LeadPipe.Infrastructure.Data.Source;

public class CatManDataSource(ICatManService cat, ISyncStateRepository state) : IDataSourceAsync<CatManDto>
{
    private readonly ICatManService _cat = cat;
    private readonly ISyncStateRepository _state = state;
    private readonly DateTime Today = DateTime.UtcNow;
    public async Task<Result<List<CatManDto>>> LoadAsync(bool _ = default)
    {
        DateTime twentyTwelve = new(2012, 1, 1);
        Result<List<CatManDto>> get = await _cat.GetAllAsync(twentyTwelve, Today);

        return get;
    }

    public async Task<Result<List<CatManDto>>> RefreshAsync(bool _ = default)
    {
        // Get most recent refresh date
        Result<SyncStateEntity> state = await _state.GetByKeyAsync(null, SyncKey.Catman);
        if (state.IsFailure)
            return await LoadAsync();

        DateTime lastSync = DateTimeOffset.FromUnixTimeSeconds(state.Value.UnixLastSyncUtc).UtcDateTime;
        Result<List<CatManDto>> result = await _cat.GetAllAsync(lastSync, Today);

        return result;
    }
}
