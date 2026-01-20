using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Infrastructure.Settings;

namespace LeadPipe.Infrastructure.Data.Transform;

internal sealed class TransformYellerReport(
    IRepositoryFactory factory,
    IVoToEntity<Plumbing, PlumbingEntity> toEntity,
    IEntityToReport<SandEntity, ReportYeller> subsToR,
    IEntityToReport<PlumbingEntity, ReportYeller> plumbToR,
    IEntityToReport<CaliperEntity, ReportYeller> caliperToR,
    IEntityToReport<CornEntity, ReportYeller> cornToR,
    IYellerSettings settings
    ) : ITransform<Plumbing, ReportYeller>
{
    private readonly IRepository<SandPlumbingLink> _sandPlumbRepo = factory.GetRepository<SandPlumbingLink>();
    private readonly IRepository<SandCaliperLink> _sandCaliperRepo = factory.GetRepository<SandCaliperLink>();
    private readonly IRepository<CaliperEntity> _caliperRepo = factory.GetRepository<CaliperEntity>();
    private readonly IRepository<CornEntity> _cornRepo = factory.GetRepository<CornEntity>();
    private readonly IRepository<SandCornLink> _cornLinksRepo = factory.GetRepository<SandCornLink>();

    private readonly IVoToEntity<Plumbing, PlumbingEntity> _voToEntity = toEntity;
    
    private readonly IEntityToReport<SandEntity, ReportYeller> _subsToR = subsToR;
    private readonly IEntityToReport<PlumbingEntity, ReportYeller> _plumbToR = plumbToR;
    private readonly IEntityToReport<CaliperEntity, ReportYeller> _caliperToR = caliperToR;
    private readonly IEntityToReport<CornEntity, ReportYeller> _cornToR = cornToR;
    
    private readonly IYellerSettings _settings = settings;

    public async Task<Result<List<ReportYeller>>> TransformAsync(List<Plumbing> data)
    {
        // *************************************
        // SubsPlumbingLinks
        // *************************************

        // Translate data to entity
        List<PlumbingEntity> plumbs = [.. data.Select(_voToEntity.Translate)];

        // Get links to subs for reporting
        var plumbingIds = plumbs.Select(p => p.Id);
        Result<List<SandPlumbingLink>> links = await _sandPlumbRepo.FindWithDetailsAsync(l => plumbingIds.Contains(l.PlumbingId));
        if (links.IsFailure)
            return Result.Failure<List<ReportYeller>>(links.Error);
        List<SandPlumbingLink> subPlumbLinks = links.Value;

        // Generate reports for plumbing and plumbing links
        // Hashset for easy lookup
        HashSet<long> plumbIds = [.. subPlumbLinks.Select(s => s.PlumbingId)];
        List<ReportYeller> plumbingReport =
            [
                .. plumbs
                    .Where(p => !plumbIds.Contains(p.Id))
                    .Select(_plumbToR.Translate),
                .. subPlumbLinks
                    .Select(s => _subsToR.Translate(s.SandEntity!))
            ];

        // *************************************
        // Calipers
        // *************************************

        // Get calls for reporting
        Result<List<CaliperEntity>> calipersResult = await _caliperRepo.FindAsync(e => e.Source == _settings.YellerCaliperSource1 || e.Source == _settings.YellerCaliperSource2);
        if (calipersResult.IsFailure)
            return Result.Failure<List<ReportYeller>>(calipersResult.Error);
        List<CaliperEntity> calipers = calipersResult.Value;

        // Get Caliper links
        var idsCaliper = calipers.Select(c => c.Id);
        Result<List<SandCaliperLink>> caliperLinksResult = await _sandCaliperRepo.FindWithDetailsAsync(l => idsCaliper.Contains(l.CaliperId));
        if (caliperLinksResult.IsFailure)
            return Result.Failure<List<ReportYeller>>(caliperLinksResult.Error);
        List<SandCaliperLink> caliperLinks = caliperLinksResult.Value;

        // Generate calls report
        // Hashset for easy lookup
        HashSet<long> caliperIds = [.. caliperLinks.Select(c => c.CaliperId)];
        List<ReportYeller> caliperReport =
            [
                .. calipers
                   .Where(c => !caliperIds.Contains(c.Id))
                   .Select(_caliperToR.Translate),
                .. caliperLinks
                    .Select(c => _subsToR.Translate(c.SandEntity!))
            ];

        // *************************************
        // Corn
        // *************************************

        // Get Corn for reporting
        Result<List<CornEntity>> cornResult = await _cornRepo.FindAsync(c => c.Source == _settings.YellerCornSource);
        if (cornResult.IsFailure)
            return Result.Failure<List<ReportYeller>>(cornResult.Error);
        List<CornEntity> corn = cornResult.Value;

        // Corn Links
        var idsCorn = corn.Select(c => c.Id);
        Result<List<SandCornLink>> cornLinksResult = await _cornLinksRepo.FindWithDetailsAsync(c => idsCorn.Contains(c.CornId));
        if (cornLinksResult.IsFailure)
            return Result.Failure<List<ReportYeller>>(cornLinksResult.Error);
        List<SandCornLink> cornLinks = cornLinksResult.Value;

        // Generate corn report
        // Hashset for easy lookup
        HashSet<long> cornIds = [.. cornLinks.Select(c => c.CornId)];
        List<ReportYeller> cornReport =
            [
                .. corn
                    .Where(c => !cornIds.Contains(c.Id))
                    .Select(_cornToR.Translate),
                .. cornLinks
                    .Select(c => _subsToR.Translate(c.SandEntity!))
            ];

        // Aggregate report lists
        List<ReportYeller> result =
            [
                .. plumbingReport,
                .. caliperReport,
                .. cornReport,
            ];

        return Result.Success(result);
    }
}
