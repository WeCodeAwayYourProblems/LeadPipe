using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Application.Manager;

public interface IUpdateManager
{
    Task<Result> Manage(Source source, bool refresh);
    Task<Result> Manage(bool refresh);
}
public sealed class UpdateManager(
    IUpdateFactory update,
    IEntityAssociationService associate,
    ISyncGate syncGate
) : IUpdateManager
{
    private readonly IUpdateFactory _update = update;
    private readonly IUpdateService<Caliper> _call = update.GetService<Caliper>();
    private readonly IUpdateService<Sandwich> _sandwich = update.GetService<Sandwich>();
    private readonly IUpdateService<CornFormula> _corn = update.GetService<CornFormula>();
    private readonly IUpdateService<Custard> _custard = update.GetService<Custard>();
    private readonly IEntityAssociationService _associate = associate;
    private readonly ISyncGate _syncGate = syncGate;

    #region Implementation

    public async Task<Result> Manage(Source source, bool refresh)
    {
        Result globals = await RunGlobals(refresh);
        if (globals.IsFailure) return globals;

        Result sources = await RunSource(source, refresh);

        // Associate
        Result associated = await AssociateIfDue();

        return Result.Combine(" | ", 
            //sources, 
            associated);
    }

    public async Task<Result> Manage(bool refresh)
    {
        Result globals = await RunGlobals(refresh);
        if (globals.IsFailure) return globals;

        // Update All Sources
        Source[] sources = [.. Enum.GetValues<Source>().Except([Source.Test, Source.Test2])];
        List<Result> results = new(sources.Length);
        foreach (var source in sources)
            results.Add(await RunSource(source, refresh));

        // Associate 
        Result associated = await AssociateIfDue();

        // Combine results 
        Result combined = Result.Combine(" | ", [.. results, associated]);
        return combined;
    }

    #endregion

    #region Helpers

    private async Task<Result> RunSource(Source source, bool refresh)
        => await RunIfDue(source, false, SyncKey.Plumbing, refresh, _update.GetService<Plumbing>(source));

    /// <summary>
    /// Whenever you need to update new global value objects, this is where they will update
    /// </summary>
    /// <param name="refresh"></param>
    /// <returns></returns>
    private async Task<Result> RunGlobals(bool refresh)
    {
        Result caliperSaved = await RunIfDue(SyncKey.Caliper, refresh, false, _call);
        if (caliperSaved.IsFailure)
            return caliperSaved;

        Result custardSaved = await RunIfDue(SyncKey.Custard, refresh, true, _custard);
        if (custardSaved.IsFailure)
            return custardSaved;

        Result sandwichSaved = await RunIfDue(SyncKey.Sandwich, refresh, true, _sandwich);
        if (sandwichSaved.IsFailure)
            return sandwichSaved;

        Result cornSaved = await RunIfDue(SyncKey.CornFormula, refresh, false, _corn);
        if (cornSaved.IsFailure)
            return cornSaved;

        return Result.Success();
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

    private async Task<Result> AssociateIfDue()
    {
        var key = SyncKey.Associate;
        bool shouldRun = await _syncGate.ShouldRunAsync(key);
        //if (!shouldRun)
        //    return Result.Success();

        Result result = await _associate.AssociateAsync();
        if (result.IsSuccess)
            await _syncGate.MarkSuccessAsync(key);
        else
            await _syncGate.MarkFailureAsync(key, result.Error);

        return result;
    }

    private async Task<Result> RunIfDue<T>(Source source, bool withDetails, SyncKey key, bool refresh, IUpdateService<T> service)
    {
        bool shouldRun = await _syncGate.ShouldRunAsync(source, key);
        if (!shouldRun)
            return Result.Success();

        Result result = await UpdatedAndSaved(refresh, withDetails, service);

        if (result.IsSuccess)
            await _syncGate.MarkSuccessAsync(source, key);
        else
            await _syncGate.MarkFailureAsync(source, key, result.Error);

        return result;
    }

    private async Task<Result> RunIfDue<T>(SyncKey key, bool refresh, bool withDetails, IUpdateService<T> service)
    {
        bool shouldRun = await _syncGate.ShouldRunAsync(key);
        //if (!shouldRun)
        //    return Result.Success();

        Result result = await UpdatedAndSaved(refresh, withDetails, service);

        if (result.IsSuccess)
            await _syncGate.MarkSuccessAsync(key);
        else
            await _syncGate.MarkFailureAsync(key, result.Error);

        return result;
    }

    #endregion
}