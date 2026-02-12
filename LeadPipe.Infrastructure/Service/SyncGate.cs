using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Settings;
using System.Collections.Immutable;

namespace LeadPipe.Infrastructure.Service;

public sealed class SyncGate(
    ISyncStateRepository repo,
    ISyncSettings settings
) : ISyncGate
{
    private readonly ISyncStateRepository _repo = repo;
    private readonly TimeSpan _noSourceInterval = TimeSpan.FromHours(settings.DefaultInterval);

    private readonly TimeSpan _defaultSourceInterval = TimeSpan.FromHours(settings.DefaultSourceInterval);
    private readonly ImmutableDictionary<Source, TimeSpan> _interval =
        Enum.GetValues<Source>().ToImmutableDictionary(
            s => s,
            s => GetTimeSpanFromSettings(s, settings));

    private static TimeSpan GetTimeSpanFromSettings(Source source, ISyncSettings settings) => source switch
    {
        Source.Test or Source.Test2 => TimeSpan.FromHours(0),

        Source.Calli => TimeSpan.FromHours(settings.CalliInterval),
        Source.Lab => TimeSpan.FromHours(settings.LabInterval),
        Source.Leaf => TimeSpan.FromHours(settings.LeafInterval),
        Source.Leased => TimeSpan.FromHours(settings.LeasedInterval),
        Source.Libacion => TimeSpan.FromHours(settings.LibacionInterval),
        Source.Pan => TimeSpan.FromHours(settings.PanInterval),
        Source.Yeller => TimeSpan.FromHours(settings.YellerInterval),
        Source.Lather => TimeSpan.FromHours(settings.LatherInterval),

        _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
    };

    public async Task<bool> ShouldRunAsync(Source source, SyncKey key)
    {
        Result<SyncStateEntity> found = await _repo.GetByKeyAsync(source, key);

        // First run: allow
        if (found.IsFailure)
            return true;

        SyncStateEntity state = found.Value;

        DateTime now = DateTime.UtcNow;

        TimeSpan interval = _interval.TryGetValue(source, out TimeSpan inter)
            ? inter
            : _defaultSourceInterval;
        bool run = now - state.LastSyncUtc >= interval;

        return run;
    }

    public async Task<bool> ShouldRunAsync(SyncKey key)
    {
        Result<SyncStateEntity> found = await _repo.GetByKeyAsync(null, key);

        if (found.IsFailure) return true;

        SyncStateEntity state = found.Value;

        DateTime now = DateTime.UtcNow;

        bool run = now - state.LastSyncUtc >= _noSourceInterval;

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
            BusinessId = BusinessId.BuildBusinessId(source, entity),
            LastSyncUtc = now.UtcDateTime,
            UnixLastSyncUtc = now.ToUnixTimeSeconds(),
            LastProcessedId = null
        };

        await _repo.UpsertRangeAsync([state]);
    }


}

