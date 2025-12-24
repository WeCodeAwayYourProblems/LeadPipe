using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Application.UpdateReportPipeline;

internal interface IPlumbingAssociationManager
{
    Task<Result> ManageAsync();
}

internal class PlumbingAssociationManager(IPlumbingAssociationService plumbs) : IPlumbingAssociationManager
{
    private readonly IPlumbingAssociationService _plumbs = plumbs;
    public async Task<Result> ManageAsync()
    {
        Result<List<Call>> callsResult = await _plumbs.GetCallAsync();
        Result<List<Sandwich>> sandResult = await _plumbs.GetSandwichAsync();
        Result<List<Plumbing>> plumbResult = await _plumbs.GetPlumbingAsync();

        Result success = Result.Combine(callsResult, sandResult, plumbResult);
        if (success.IsFailure)
            return success;

        return await _plumbs.SaveAllAsync(plumbResult.Value, sandResult.Value, callsResult.Value);
    }
}
