using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
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

    public async Task<bool> ShouldRunAsync(Source source, SyncKey key)
    {
        Result<SyncStampEntity> found = await _repo.GetByKeyAsync(source, key);

        // First run: allow
        if (found.IsFailure)
            return true;

        SyncStampEntity state = found.Value;

        DateTimeOffset now = DateTime.UtcNow;

        TimeSpan interval = _interval.TryGetValue(source, out TimeSpan inter)
            ? inter
            : _defaultSourceInterval;
        DateTimeOffset syncDate = DateTimeOffset.FromUnixTimeSeconds(state.UnixSyncUtc);
        bool run = now - syncDate >= interval;

        return run;
    }

    public async Task<bool> ShouldRunAsync(SyncKey key)
    {
        Result<SyncStampEntity> found = await _repo.GetByKeyAsync(null, key);

        if (found.IsFailure) return true;

        SyncStampEntity state = found.Value;

        DateTimeOffset now = DateTime.UtcNow;

        TimeSpan syncstatetiming = now - DateTimeOffset.FromUnixTimeSeconds(state.UnixSyncUtc);
        TimeSpan interval = key.Value == SyncKey.Associate.Value
            ? _associationInterval
            : _noSourceInterval;

        bool run = syncstatetiming >= interval;

        return run;
    }

    public async Task MarkSuccessAsync(Source source, SyncKey entity) => await UpsertAsync(source, entity, true);

    public async Task MarkSuccessAsync(SyncKey entity) => await UpsertAsync(null, entity, true);

    // don't currently persist error state.
    public async Task MarkFailureAsync(Source source, SyncKey entity, string error) => await UpsertAsync(source, entity, false);

    public async Task MarkFailureAsync(SyncKey entity, string error) => await UpsertAsync(null, entity, false);

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

