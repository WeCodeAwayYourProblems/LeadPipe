namespace LeadPipe.Infrastructure.Service;

using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Settings;

public sealed class SyncGate(
    ISyncStateRepository repo,
    ISyncSettings settings
) : ISyncGate
{
    private readonly ISyncStateRepository _repo = repo;
    private readonly TimeSpan _interval = TimeSpan.FromHours(settings.HourInterval);

    public async Task<bool> ShouldRunAsync(Source source, SyncKey entity)
    {
        BusinessId id = BuildBusinessId(source, entity);

        Result<SyncStateEntity> found = await _repo.GetByIdAsync(id);

        // First run → allow
        if (found.IsFailure)
            return true;

        SyncStateEntity state = found.Value;

        DateTime now = DateTime.UtcNow;

        bool run = now - state.LastSyncUtc >= _interval;

        return run;
    }

    public async Task<bool> ShouldRunAsync(SyncKey entity)
    {
        BusinessId id = BuildBusinessId(null, entity);

        Result<SyncStateEntity> found = await _repo.GetByIdAsync(id);

        if (found.IsFailure) return true;

        SyncStateEntity state = found.Value;

        DateTime now = DateTime.UtcNow;

        bool run = now - state.LastSyncUtc >= _interval;

        return run;
    }

    public async Task MarkSuccessAsync(Source source, SyncKey entity)
    {
        await UpsertAsync(source, entity);
    }

    public async Task MarkSuccessAsync(SyncKey entity)
    {
        await UpsertAsync(null, entity);
    }

    public async Task MarkFailureAsync(Source source, SyncKey entity, string error)
    {
        // don't currently persist error state.
        await UpsertAsync(source, entity);
    }

    public async Task MarkFailureAsync(SyncKey entity, string error)
    {
        await UpsertAsync(null, entity);
    }

    private async Task UpsertAsync(Source? source, SyncKey entity)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        SyncStateEntity state = new()
        {
            BusinessId = BuildBusinessId(source, entity),
            LastSyncUtc = now.UtcDateTime,
            UnixLastSyncUtc = now.ToUnixTimeSeconds(),
            LastProcessedId = null
        };

        await _repo.UpsertRangeAsync([state]);
    }

    private static BusinessId BuildBusinessId(Source? source, SyncKey entity)
    {
        string scope = source is null 
            ? "global" 
            : source.ToString()!.ToLowerInvariant();

        return BusinessId.From($"{scope}:{entity.Value}");
    }
}

