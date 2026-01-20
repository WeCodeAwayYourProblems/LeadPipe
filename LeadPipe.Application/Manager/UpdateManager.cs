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
    IUpdateSourceFactory update,
    IUpdateService<Caliper> call,
    IUpdateService<Sandwich> sandwich,
    IEntityAssociationService plumb
    ) : IUpdateManager
{
    private readonly IUpdateSourceFactory _update = update;
    private readonly IUpdateService<Caliper> _call = call;
    private readonly IUpdateService<Sandwich> _sandwich = sandwich;
    private readonly IEntityAssociationService _plumb = plumb;
    
    public async Task<Result> Manage(Source source, bool refresh)
    {
        // Caliper
        Result callSaved = await UpdatedAndSaved(refresh, _call);
        if (callSaved.IsFailure)
            return callSaved;

        // Sandwich
        Result sandwichSaved = await UpdatedAndSaved(refresh, _sandwich);
        if (sandwichSaved.IsFailure)
            return sandwichSaved;

        // Plumbing
        Result plumbingSaved = await UpdatePlumb(source, refresh);
        if (plumbingSaved.IsFailure)
            return plumbingSaved;

        // Associate
        Result associated = await _plumb.AssociateAsync();
        if (associated.IsFailure)
            return associated;

        return Result.Combine(" | ", callSaved, sandwichSaved, plumbingSaved, associated);
    }

    public async Task<Result> Manage(bool refresh)
    {
        // Caliper
        Result callSaved = await UpdatedAndSaved(refresh, _call);
        if (callSaved.IsFailure)
            return callSaved;

        // Sandwich
        Result sandwichSaved = await UpdatedAndSaved(refresh, _sandwich);
        if (sandwichSaved.IsFailure)
            return sandwichSaved;

        // Update All Sources
        Source[] sources = Enum.GetValues<Source>();
        List<Result> results = new(sources.Length);
        foreach (var source in sources)
        {
            if (source == Source.Test || source == Source.Test2)
                continue;
            Result plumb = await UpdatePlumb(source, refresh);
            results.Add(plumb);
        }

        // Combine results 
        Result combined = Result.Combine(" | ", [.. results]);
        if (combined.IsFailure)
            return combined;

        // Associate
        Result associated = await _plumb.AssociateAsync();

        return associated;
    }
    
    private async Task<Result> UpdatePlumb(Source source, bool refresh)
    {
        IUpdateService<Plumbing> plumbing = _update.GetService(source);
        var result = await UpdatedAndSaved(refresh, plumbing);
        return result;
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
}