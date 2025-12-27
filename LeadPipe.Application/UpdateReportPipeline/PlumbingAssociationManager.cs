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
        return await _plumbs.SaveLinksAsync();
    }
}
