using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Data.Transform;

public sealed class TransformPlumbingReport(
    IRepository<SandPlumbingLink> repo,
    IVoToEntity<Plumbing, PlumbingEntity> voToEntity,
    IEntityToReport<SandPlumbingLink, ReportPlumbing> eToR
    ) : ITransform<Plumbing, ReportPlumbing>
{
    private readonly IRepository<SandPlumbingLink> _repo = repo;
    private readonly IVoToEntity<Plumbing, PlumbingEntity> _voToEntity = voToEntity;
    private readonly IEntityToReport<SandPlumbingLink, ReportPlumbing> _eToR = eToR;
    public async Task<Result<List<ReportPlumbing>>> TransformAsync(List<Plumbing> data)
    {
        // Translate plumbing to plumbingentity
        List<PlumbingEntity> plumbingEntities = [.. data.Select(_voToEntity.Translate)];

        // Get links to the plumbing
        var plumbingIds = plumbingEntities.Select(e => e.Id);
        Result<List<SandPlumbingLink>> linkResult = await _repo.FindWithDetailsAsync(l => plumbingIds.Contains(l.PlumbingId));

        // Check Success
        List<SandPlumbingLink>? links = linkResult.IsSuccess
            ? linkResult.Value
            : null;
        if (links is null)
            return Result.Failure<List<ReportPlumbing>>(linkResult.Error);

        // Ensure the list is not null
        List<SandPlumbingLink> subsLinks = links!;

        // Turn subsplumbinglinks into a hashset of plumbingids for fast lookup
        HashSet<long> ids = [.. subsLinks.Select(e => e.PlumbingId)];

        // Turn empty PlumbingEntities into subsplumbinglinks
        List<SandPlumbingLink> unfoundPlumbing =
            [.. plumbingEntities
                .Where(e => !ids.Contains(e.Id)) // We are creating a partition
                .Select(e => new SandPlumbingLink { PlumbingEntity = e, SandId = 0})
            ];

        List<ReportPlumbing> result =
            [
                .. subsLinks.Select(_eToR.Translate),
                .. unfoundPlumbing.Select(_eToR.Translate)
            ];

        return result;
    }
}
