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
    IVoToEntity<Plumbing, PlumbingEntity> toEntity,
    IEntityToReport<SubsEntity, ReportYeller> eToR,
    IYellerSettings settings
    ) : ITransform<Plumbing, ReportYeller>
{
    private readonly ISubsPlumbingLinkRepository _subsPlumbRepo = spRepo;
    private readonly ISubsCallLinkRepository _subsCallRepo = subsCallRepo;
    private readonly ICallRepository _callRepo = callRepo;
    private readonly IVoToEntity<Plumbing, PlumbingEntity> _voToEntity = toEntity;
    private readonly IEntityToReport<SubsEntity, ReportYeller> _eToR = eToR;
    private readonly IEntityToReport<SubsPlumbingLink, ReportYeller> _spLinkToR = spLinkToR;
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

        // Check success
        List<SubsPlumbingLink>? spLinks = links.IsSuccess
            ? links.Value
            : null;
        if (spLinks is null)
            return Result.Failure<List<ReportYeller>>(links.Error);
        List<SubsPlumbingLink> subPlumbLinks = spLinks!; // Ensure the list is not null

        // *************************************
        // Calls
        // *************************************

        // Get calls for reporting
        Result<List<CallEntity>> callsResult = await _callRepo.FindAsync(e => e.Source == _settings.YellerCallSource1 || e.Source == _settings.YellerCallSource2);
        List<CallEntity>? callsList = callsResult.IsSuccess // Check success
            ? callsResult.Value
            : null;
        if (callsList is null)
            return Result.Failure<List<ReportYeller>>(callsResult.Error);
        List<CallEntity> calls = callsList!; // Ensure the list is not null

        // Get Call links
        Result<List<CallSubsLink>> callLinksResult = await _subsCallRepo.GetAllWithDetailsAsync(calls);
        List<CallSubsLink>? cl = callLinksResult.IsSuccess // Check success
            ? callLinksResult.Value
            : null;
        List<CallSubsLink> callLinks = cl!; // Ensure the list is not null

        // Generate report
        List<ReportYeller> spLinksReport =
            [.. subPlumbLinks
                .Where(s => s.SubsEntity is not null)
                .Select(s=>_eToR.Translate(s.SubsEntity!)),
            ];
        List<ReportYeller> callLinksReport =
            [.. callLinks
                .Where(c => c.SubsEntity is not null)
                .Select(c => _eToR.Translate(c.SubsEntity!)),
            ];

        return Result.Success<List<ReportYeller>>([.. spLinksReport, .. callLinksReport]);
    }

}
