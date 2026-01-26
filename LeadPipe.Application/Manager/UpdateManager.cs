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
    IEntityAssociationService plumb
) : IUpdateManager
{
    private readonly IUpdateFactory _update = update;
    private readonly IUpdateService<Caliper> _call = update.GetService<Caliper>();
    private readonly IUpdateService<Sandwich> _sandwich = update.GetService<Sandwich>();
    private readonly IUpdateService<Custard> _custard = update.GetService<Custard>();
    private readonly IEntityAssociationService _plumb = plumb;

    private IUpdateService<Plumbing>? Plumbing { get; set; }

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

        return Result.Combine(" | ", 
            caliperSaved, 
            custardSaved,
            sandwichSaved, 
            plumbingSaved, 
            associated);
    }

    public async Task<Result> Manage(bool refresh)
    {
        // Caliper
        Result callSaved = await UpdatedAndSaved(refresh, _call);
        if (callSaved.IsFailure)
            return callSaved;

        // Custard
        Result custardSaved = await UpdatedAndSaved(refresh, _custard);
        if (custardSaved.IsFailure)
            return custardSaved;

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
        Plumbing ??= _update.GetService<Plumbing>(source);
        var result = await UpdatedAndSaved(refresh, Plumbing);
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