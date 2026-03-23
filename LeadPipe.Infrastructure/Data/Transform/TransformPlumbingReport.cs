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
        List<long> plumbingIds = [.. plumbingEntities.Select(e => e.Id)];
        Result<List<SandPlumbingLink>> linkResult = await _repo.FindWithDetailsAsync(l => plumbingIds.Contains(l.PlumbingId));

        // Check Success
        if (linkResult.IsFailure)
            return Result.Failure<List<ReportPlumbing>>(linkResult.Error);
        List<SandPlumbingLink> links = linkResult.Value;

        // Turn sandplumbinglinks into a hashset of plumbingids for fast lookup
        HashSet<long> ids = [.. links.Select(e => e.PlumbingId)];

        // Turn empty PlumbingEntities into sandplumbinglinks
        List<SandPlumbingLink> unfoundPlumbing =
            [.. plumbingEntities
                .Where(e => !ids.Contains(e.Id)) // We are creating a partition
                .Select(e => new SandPlumbingLink
                {
                    PlumbingId = e.Id,
                    PlumbingEntity = e,
                    SandId = 0,
                    UnixMatchDate = e.UnixDate,
                    MatchingPhone = e.PhoneNumber.Number
                })
            ];

        List<ReportPlumbing> result =
            [
                .. links.Select(_eToR.Translate),
                .. unfoundPlumbing.Select(_eToR.Translate)
            ];

        return result;
    }
}
