using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Data.Transform;

public sealed class TransformPlumbingReport(
    ISubsPlumbingLinkRepository repo,
    IVoToEntity<Plumbing, PlumbingEntity> voToEntity,
    IEntityToReport<SubsEntity, ReportPlumbing> eToR
    ) : ITransform<Plumbing, ReportPlumbing>
{
    private readonly ISubsPlumbingLinkRepository _repo = repo;
    private readonly IVoToEntity<Plumbing, PlumbingEntity> _voToEntity = voToEntity;
    private readonly IEntityToReport<SubsEntity, ReportPlumbing> _eToR = eToR;
    public async Task<Result<List<ReportPlumbing>>> TransformAsync(List<Plumbing> data)
    {
        // Translate plumbing to plumbingentity
        List<PlumbingEntity> plumbingEntities = [.. data.Select(_voToEntity.Translate)];

        // Get links to the plumbing
        Result<List<SubsPlumbingLink>> linkResult = await _repo.GetAllWithDetailsAsync(plumbingEntities);

        // Check Success
        List<SubsPlumbingLink>? links = linkResult.IsSuccess
            ? linkResult.Value
            : null;
        if (links is null)
            return Result.Failure<List<ReportPlumbing>>(linkResult.Error);

        // Ensure the list is not null
        List<SubsPlumbingLink> subsLinks = links!;

        // Turn subsplumbinglinks into a hashset of plumbingids for fast lookup
        HashSet<long> ids = [.. subsLinks.Select(e => e.PlumbingId)];

        // Turn empty PlumbingEntities into subsplumbinglinks
        List<SubsPlumbingLink> unfoundPlumbing =
            [.. plumbingEntities
                .Where(e => !ids.Contains(e.Id)) // We are creating a partition
                .Select(e => new SubsPlumbingLink { PlumbingEntity = e, SubsId = 0})
            ];

        List<ReportPlumbing> result =
            [
                .. subsLinks.Select(s => _eToR.Translate(s.SubsEntity!)),
                .. unfoundPlumbing.Select(p => _eToR.Translate(p.SubsEntity))
            ];

        return result;
    }
}
