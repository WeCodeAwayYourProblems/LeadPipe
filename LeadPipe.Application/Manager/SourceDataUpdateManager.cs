using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Core;
using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Application.Manager;

public interface ISourceDataUpdateManager
{
    Task<Result> Manage(ForceRunRefresh frr);
    Task<Result> Manage(ForceRunRefresh frr, params Source[] sources);
}
public class SourceDataUpdateManager : ISourceDataUpdateManager
{
    private readonly Source[] _validSources;
    private readonly ISyncGate _syncGate;
    private readonly Dictionary<Source, IUpdateService<Plumbing>> _services;

    public SourceDataUpdateManager(
        IUpdateFactory updateFactory,
        ISyncGate syncGate
    )
    {
        _syncGate = syncGate;
        _validSources = [.. Enum.GetValues<Source>().Except([
            Source.Test, 
            Source.Test2, 
            Source.Leaf,
            Source.Yeller
        ])];
        _services = _validSources.ToDictionaryFast(
        s => s,
        updateFactory.GetService<Plumbing>
    );
    }

    public Task<Result> Manage(ForceRunRefresh frr) => Manage(frr, _validSources);

    public async Task<Result> Manage(ForceRunRefresh frr, params Source[] sources)
    {
        foreach (var source in sources)
        {
            var result = await RunIfDue(source, frr, _services[source], _syncGate);
            if (result.IsFailure)
                return result;
        }
        return Result.Success();
    }

    private static async Task<Result> RunIfDue<T>(Source source, ForceRunRefresh frr, IUpdateService<T> service, ISyncGate syncGate)
    {
        bool shouldRun = await syncGate.ShouldRunAsync(source, service.SyncKey);
        if (!shouldRun && !frr.ForceRun)
            return Result.Success();

        Result result = await UpdatedAndSaved(frr.Refresh, false, service);

        if (result.IsSuccess)
            await syncGate.MarkSuccessAsync(source, service.SyncKey);
        else
            await syncGate.MarkFailureAsync(source, service.SyncKey);

        return result;
    }

    private static async Task<Result> UpdatedAndSaved<T>(bool refresh, bool withDetails, IUpdateService<T> updateService)
    {
        Result<List<T>> updateData = refresh
            ? await updateService.UpdateDataAsync(withDetails)
            : await updateService.GetDataAsync(withDetails);
        Result savedData = updateData.IsSuccess
            ? await updateService.SaveDataAsync(updateData.Value)
            : updateData;
        return savedData;
    }
}