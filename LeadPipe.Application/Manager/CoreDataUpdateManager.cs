using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Application.Manager;

public interface ICoreDataUpdateManager
{
    // Refreshes all
    Task<Result> Manage(bool refresh);
    Task<Result> Manage(bool refresh, params SyncKey[] keys);
}

internal class CoreDataUpdateManager : ICoreDataUpdateManager
{
    private readonly Dictionary<SyncKey, Func<bool, Task<Result>>> _handlers;

    public CoreDataUpdateManager(
        ISyncGate syncGate,
        IUpdateFactory updateFactory
    )
    {
        var caliper = updateFactory.GetService<Caliper>();
        var custard = updateFactory.GetService<Custard>();
        var sandwich = updateFactory.GetService<Sandwich>();
        var cornFormula = updateFactory.GetService<CornFormula>();

        _handlers = new()
        {
            { SyncKey.Caliper, refresh => RunIfDue(refresh, false, caliper, syncGate) },
            { SyncKey.Custard, refresh => RunIfDue(refresh, true, custard, syncGate) },
            { SyncKey.Sandwich, refresh => RunIfDue(refresh, true, sandwich, syncGate) },
            { SyncKey.CornFormula, refresh => RunIfDue(refresh, false, cornFormula, syncGate) },
        };
    }
    
    private static string InvalidKey(SyncKey key) => $"Invalid key: {key}";

    public Task<Result> Manage(bool refresh) => Manage(refresh, [.. _handlers.Keys]);

    public async Task<Result> Manage(bool refresh, params SyncKey[] keys)
    {
        foreach (var key in keys)
        {
            if (!_handlers.TryGetValue(key, out Func<bool, Task<Result>>? handler))
                return Result.Failure(InvalidKey(key));

            var result = await handler(refresh);

            if (result.IsFailure)
                return result;
        }
        return Result.Success();
    }

    private static async Task<Result> RunIfDue<T>(bool refresh, bool withDetails, IUpdateService<T> service, ISyncGate syncGate)
    {
        bool shouldRun = await syncGate.ShouldRunAsync(null, service.SyncKey);
        if (!shouldRun)
            return Result.Success();

        Result result = await UpdatedAndSaved(refresh, withDetails, service);

        if (result.IsSuccess)
            await syncGate.MarkSuccessAsync(null, service.SyncKey);
        else
            await syncGate.MarkFailureAsync(null, service.SyncKey);

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
