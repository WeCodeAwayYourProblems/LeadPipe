using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Application.Manager;

public interface IUpdateManager
{
    Task<Result> Manage(Source source, bool refresh, bool forceRun);
    Task<Result> Manage(bool refresh, bool forceRun);
}
internal sealed class UpdateManager(
    ISourceDataUpdateManager sourceData,
    ICoreDataUpdateManager coreData,
    IAssociationManager associate
    ) : IUpdateManager
{
    readonly ISourceDataUpdateManager _source = sourceData;
    readonly ICoreDataUpdateManager _core = coreData;
    readonly IAssociationManager _associate = associate;
    public async Task<Result> Manage(Source source, bool refresh, bool forceRun)
    {
        var core = await _core.Manage(refresh, forceRun);
        var sourceData = await _source.Manage(refresh, source);
        var associate = await _associate.Manage();
        var combined = Result.Combine(core, sourceData, associate);
        return combined;
    }

    public async Task<Result> Manage(bool refresh, bool forceRun)
    {
        var core = await _core.Manage(refresh, forceRun);
        var sourceData = await _source.Manage(refresh);
        var associate = await _associate.Manage();
        var combined = Result.Combine(core, sourceData, associate);
        return combined;
    }
}