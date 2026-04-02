using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Application.Manager;

public interface ISourceDataUpdateManager
{
    Task<Result> Manage(bool refresh);
    Task<Result> Manage(bool refresh, params Source[] sources);
}
public class SourceDataUpdateManager(
    IUpdateFactory updateFactory,
    ISyncGate syncGate
    ) : ISourceDataUpdateManager
{
    private readonly IUpdateFactory _updateFactory = updateFactory;
    private readonly ISyncGate _syncGate = syncGate;
    private static readonly Source[] ValidSources = [.. Enum.GetValues<Source>().Except([Source.Test,Source.Test2])];

    public Task<Result> Manage(bool refresh) => Manage(refresh, ValidSources);

    public async Task<Result> Manage(bool refresh, params Source[] sources)
    {
        foreach(var source in sources)
        {
            if (source == Source.Test || source == Source.Test2)
                continue;
            var result = await RunIfDue(source, refresh, _updateFactory.GetService<Plumbing>(source), _syncGate);
            if (result.IsFailure)
                return result;
        }
        return Result.Success();
    }

    private static async Task<Result> RunIfDue<T>(Source source, bool refresh, IUpdateService<T> service, ISyncGate syncGate)
    {
        bool shouldRun = await syncGate.ShouldRunAsync(source, SyncKey.Plumbing);
        if (!shouldRun)
            return Result.Success();

        Result result = await UpdatedAndSaved(refresh, false, service);

        if (result.IsSuccess)
            await syncGate.MarkSuccessAsync(source, SyncKey.Plumbing);
        else
            await syncGate.MarkFailureAsync(source, SyncKey.Plumbing);

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