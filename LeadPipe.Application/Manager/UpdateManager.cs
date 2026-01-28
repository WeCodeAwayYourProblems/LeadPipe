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
    private readonly IUpdateService<Custard> _custard = update.GetService<Custard>();
    private readonly IEntityAssociationService _associate = associate;
    private readonly ISyncGate _syncGate = syncGate;

    private const string _associateStr = "Associate"; // DO NOT change this string if you've migrated any crud dbs

    public async Task<Result> Manage(Source source, bool refresh)
    {
        Result globals = await RunGlobals(refresh);
        if (globals.IsFailure) return globals;

        Result sources = await RunSource(source, refresh);

        // Associate
        Result associated = await AssociateIfDue();

        return Result.Combine(" | ", sources, associated);
    }

    public async Task<Result> Manage(bool refresh)
    {
        // UpdateGlobals
        var globals = await RunGlobals(refresh);
        if (globals.IsFailure) return globals;

        // Update All Sources
        Source[] sources = Enum.GetValues<Source>();
        List<Result> results = new(sources.Length);
        foreach (var source in sources)
            results.Add(await RunSource(source, refresh));

        // Associate 
        Result associated = await AssociateIfDue();

        // Combine results 
        Result combined = Result.Combine(" | ", [.. results, associated]);
        return combined;
    }

    private async Task<Result> RunSource(Source source, bool refresh) 
        => await RunIfDue(source, nameof(Plumbing), refresh, _update.GetService<Plumbing>(source));

    /// <summary>
    /// With new global entities that need to be updated, this is the method that will update
    /// </summary>
    /// <param name="refresh"></param>
    /// <returns></returns>
    private async Task<Result> RunGlobals(bool refresh)
    {
        Result caliperSaved = await RunIfDue(nameof(Caliper), refresh, _call);
        if (caliperSaved.IsFailure) return caliperSaved;

        Result custardSaved = await RunIfDue(nameof(Custard), refresh, _custard);
        if (custardSaved.IsFailure) return custardSaved;

        Result sandwichSaved = await RunIfDue(nameof(Sandwich), refresh, _sandwich);
        if (sandwichSaved.IsFailure) return sandwichSaved;

        return Result.Success();
    }

    private static async Task<Result> UpdatedAndSaved<T>(bool refresh, IUpdateService<T> updateService)
    {
        Result<List<T>> updateData = refresh
            ? await updateService.UpdateDataAsync()
            : await updateService.GetDataAsync();
        Result savedData = updateData.IsSuccess
            ? await updateService.SaveDataAsync(updateData.Value)
            : updateData;
        return savedData;
    }

    private async Task<Result> AssociateIfDue()
    {
        if (!await _syncGate.ShouldRunAsync(_associateStr))
            return Result.Success();

        Result result = await _associate.AssociateAsync();
        if (result.IsSuccess)
            await _syncGate.MarkSuccessAsync(_associateStr);
        else
            await _syncGate.MarkFailureAsync(_associateStr, result.Error);

        return result;
    }

    private async Task<Result> RunIfDue<T>(Source source, string entity, bool refresh, IUpdateService<T> service)
    {
        bool shouldRun = await _syncGate.ShouldRunAsync(source, entity);
        if (!shouldRun)
            return Result.Success();

        Result result = await UpdatedAndSaved(refresh, service);

        if (result.IsSuccess)
            await _syncGate.MarkSuccessAsync(source, entity);
        else
            await _syncGate.MarkFailureAsync(source, entity, result.Error);

        return result;
    }

    private async Task<Result> RunIfDue<T>(string entity, bool refresh, IUpdateService<T> service)
    {
        bool shouldRun = await _syncGate.ShouldRunAsync(entity);
        if (!shouldRun)
            return Result.Success();

        Result result = await UpdatedAndSaved(refresh, service);

        if (result.IsSuccess)
            await _syncGate.MarkSuccessAsync(entity);
        else
            await _syncGate.MarkFailureAsync(entity, result.Error);

        return result;
    }

}