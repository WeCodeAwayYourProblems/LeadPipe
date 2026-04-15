using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Application.Manager;

public interface IAssociationManager
{
    Task<Result> Manage(ForceRunRefresh frr);
}
internal class AssociationManager(
    ISyncGate syncGate,
    IEntityAssociationService associationService
    ) : IAssociationManager
{
    readonly ISyncGate _syncGate = syncGate;
    readonly IEntityAssociationService _associate = associationService;
    public Task<Result> Manage(ForceRunRefresh frr) => AssociateIfDue(frr);
    private async Task<Result> AssociateIfDue(ForceRunRefresh frr)
    {
        var key = SyncKey.Associate;
        bool shouldRun = await _syncGate.ShouldRunAsync(null, key);
        if (!shouldRun && !frr.ForceRun)
            return Result.Success();

        Result result = await _associate.AssociateAsync();
        if (result.IsSuccess)
            await _syncGate.MarkSuccessAsync(null, key);
        else
            await _syncGate.MarkFailureAsync(null, key);

        return result;
    }
}