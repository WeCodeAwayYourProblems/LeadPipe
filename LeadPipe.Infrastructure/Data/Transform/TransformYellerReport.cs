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
    ISubsPlumbingLinkRepository spRepo,
    ISubsCallLinkRepository subsCallRepo,
    ICallRepository callRepo,
    ICornRepository cornRepo,
    ISubsCornLinkRepository cornLinksRepo,
    IVoToEntity<Plumbing, PlumbingEntity> toEntity,
    IEntityToReport<SubsEntity, ReportYeller> subsToR,
    IEntityToReport<PlumbingEntity, ReportYeller> plumbToR,
    IEntityToReport<CallEntity, ReportYeller> callToR,
    IEntityToReport<CornEntity, ReportYeller> cornToR,
    IYellerSettings settings
    ) : ITransform<Plumbing, ReportYeller>
{
    private readonly ISubsPlumbingLinkRepository _subsPlumbRepo = spRepo;
    private readonly ISubsCallLinkRepository _subsCallRepo = subsCallRepo;
    private readonly ICallRepository _callRepo = callRepo;
    private readonly ICornRepository _cornRepo = cornRepo;
    private readonly ISubsCornLinkRepository _cornLinksRepo = cornLinksRepo;
    private readonly IVoToEntity<Plumbing, PlumbingEntity> _voToEntity = toEntity;
    private readonly IEntityToReport<SubsEntity, ReportYeller> _subsToR = subsToR;
    private readonly IEntityToReport<PlumbingEntity, ReportYeller> _plumbToR = plumbToR;
    private readonly IEntityToReport<CallEntity, ReportYeller> _callToR = callToR;
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
        Result<List<SubsPlumbingLink>> links = await _subsPlumbRepo.GetAllWithDetailsAsync(plumbs);
        if (links.IsFailure)
            return Result.Failure<List<ReportYeller>>(links.Error);
        List<SubsPlumbingLink> subPlumbLinks = links.Value;

        // Generate reports for plumbing and plumbing links
        // Hashset for easy lookup
        HashSet<long> plumbIds = [.. subPlumbLinks.Select(s => s.PlumbingId)];
        List<ReportYeller> plumbingReport =
            [
                .. plumbs
                    .Where(p => !plumbIds.Contains(p.Id))
                    .Select(_plumbToR.Translate),
                .. subPlumbLinks
                    .Select(s => _subsToR.Translate(s.SubsEntity!))
            ];

        // *************************************
        // Calls
        // *************************************

        // Get calls for reporting
        Result<List<CallEntity>> callsResult = await _callRepo.FindAsync(e => e.Source == _settings.YellerCallSource1 || e.Source == _settings.YellerCallSource2);
        if (callsResult.IsFailure)
            return Result.Failure<List<ReportYeller>>(callsResult.Error);
        List<CallEntity> calls = callsResult.Value;

        // Get Call links
        Result<List<SubsCallLink>> callLinksResult = await _subsCallRepo.GetAllWithDetailsAsync(calls);
        if (callLinksResult.IsFailure)
            return Result.Failure<List<ReportYeller>>(callLinksResult.Error);
        List<SubsCallLink> callLinks = callLinksResult.Value;

        // Generate calls report
        // Hashset for easy lookup
        HashSet<long> callIds = [.. callLinks.Select(c => c.CallId)];
        List<ReportYeller> callReport =
            [
                .. calls
                   .Where(c => !callIds.Contains(c.Id))
                   .Select(_callToR.Translate),
                .. callLinks
                    .Select(c => _subsToR.Translate(c.SubsEntity!))
            ];

        // *************************************
        // Corn
        // *************************************

        // Get Corn for reporting
        Result<List<CornEntity>> cornResult = await _cornRepo.FindAsync(c => c.Source == Domain.ValueObjects.Source.Yeller);
        if (cornResult.IsFailure)
            return Result.Failure<List<ReportYeller>>(cornResult.Error);
        List <CornEntity> corn = cornResult.Value;

        // Corn Links
        Result<List<SubsCornLink>> cornLinksResult = await _cornLinksRepo.GetAllWithDetailsAsync(corn);
        if (cornLinksResult.IsFailure)
            return Result.Failure<List<ReportYeller>>(cornLinksResult.Error);
        List <SubsCornLink> cornLinks = cornLinksResult.Value;

        // Generate corn report
        // Hashset for easy lookup
        HashSet<long> cornIds = [.. cornLinks.Select(c => c.CornId)];
        List<ReportYeller> cornReport =
            [
                .. corn
                    .Where(c => !cornIds.Contains(c.Id))
                    .Select(_cornToR.Translate),
                .. cornLinks
                    .Select(c => _subsToR.Translate(c.SubsEntity!))
            ];

        // Aggregate report lists
        List<ReportYeller> result =
            [
                .. plumbingReport,
                .. callReport,
                .. cornReport,
            ];

        return Result.Success(result);
    }
}
