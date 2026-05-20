using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Application.Manager;

public interface IUpdateManager
{
    Task<Result> Manage(Source source, ForceRunRefresh frr);
    Task<Result> Manage(ForceRunRefresh frr);
}
public record ForceRunRefresh(bool ForceRun, bool Refresh);
internal sealed class UpdateManager(
    ISourceDataUpdateManager sourceData,
    ICoreDataUpdateManager coreData,
    IAssociationManager associate
    ) : IUpdateManager
{
    readonly ISourceDataUpdateManager _source = sourceData;
    readonly ICoreDataUpdateManager _core = coreData;
    readonly IAssociationManager _associate = associate;
    public async Task<Result> Manage(Source source, ForceRunRefresh frr)
    {
        var core = await _core.Manage(frr);
        var sourceData = await _source.Manage(frr, source);
        var associate = await _associate.Manage(frr);
        var combined = Result.Combine(
            core,
            sourceData
        , associate
        );
        return combined;
    }

    public async Task<Result> Manage(ForceRunRefresh frr)
    {
        var core = await _core.Manage(frr);
        var sourceData = await _source.Manage(frr);
        var associate = await _associate.Manage(frr);
        var combined = Result.Combine(
            core,
            sourceData
        , associate
        );
        return combined;
    }
}