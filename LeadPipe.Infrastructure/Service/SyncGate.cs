using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Repository;
using LeadPipe.Infrastructure.Settings;
using System.Collections.Immutable;

namespace LeadPipe.Infrastructure.Service;

public sealed class SyncGate(
    ISyncStampRepository repo,
    ISyncSettings settings
) : ISyncGate
{
    private readonly ISyncStampRepository _repo = repo;
    private readonly TimeSpan _noSourceInterval = TimeSpan.FromHours(settings.DefaultInterval);
    private readonly TimeSpan _associationInterval = TimeSpan.FromHours(settings.DefaultAssociationInterval);
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

    #region Should Run

    public async Task<bool> ShouldRunAsync(Source? source, SyncKey key)
    {
        if (source is null)
            return await ShouldRunAsync(key);

        Result<SyncStampEntity> found = await _repo.GetByKeyAsync(source, key);

        if (found.IsFailure) return true;
        bool run = ShouldRun((Source)source!, found.Value);

        return run;
    }

    private async Task<bool> ShouldRunAsync(SyncKey key)
    {
        Result<SyncStampEntity> found = await _repo.GetByKeyAsync(null, key);

        if (found.IsFailure) return true;
        bool run = ShouldRun(key, found.Value);

        return run;
    }

    private bool ShouldRun(Source source, SyncStampEntity found)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        TimeSpan resetInterval = _interval.TryGetValue(source, out TimeSpan inter)
            ? inter
            : _defaultSourceInterval;

        DateTimeOffset syncDate = DateTimeOffset.FromUnixTimeSeconds(found.UnixSyncUtc);
        TimeSpan timeSinceSync = now - syncDate;

        bool run = timeSinceSync >= resetInterval || found.SuccessState is false;

        return run;
    }

    private bool ShouldRun(SyncKey key, SyncStampEntity found)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        TimeSpan syncstatetiming = now - DateTimeOffset.FromUnixTimeSeconds(found.UnixSyncUtc);

        TimeSpan interval = key.Value == SyncKey.Associate.Value
            ? _associationInterval
            : _noSourceInterval;

        bool run = syncstatetiming >= interval || found.SuccessState is false;

        return run;
    }

    #endregion

    public async Task MarkSuccessAsync(Source? source, SyncKey entity) => await UpsertAsync(source, entity, true);

    public async Task MarkFailureAsync(Source? source, SyncKey entity) => await UpsertAsync(source, entity, false);

    private async Task UpsertAsync(Source? source, SyncKey entity, bool successState)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        SyncStampEntity state = new()
        {
            Id = default,
            Key = entity,
            Source = source,
            UnixSyncUtc = now.ToUnixTimeSeconds(),
            SuccessState = successState
        };

        await _repo.UpsertAsync(state);
    }

}

