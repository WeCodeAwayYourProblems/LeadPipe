using CSharpFunctionalExtensions;
using LeadPipe.Core;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Data.Transform;

public sealed class TransformPlumbingReport(
    IRepository<SandPlumbingLink> repo,
    IRepository<PlumbingEntity> plumbs,
    IEntityToReport<SandPlumbingLink, ReportPlumbing> eToR
    ) : ITransform<Plumbing, ReportPlumbing>
{
    private readonly IRepository<SandPlumbingLink> _repo = repo;
    private readonly IRepository<PlumbingEntity> _plumbs = plumbs;
    private readonly IEntityToReport<SandPlumbingLink, ReportPlumbing> _eToR = eToR;
    public async Task<Result<List<ReportPlumbing>>> TransformAsync(List<Plumbing> data)
    {
        // Retrieve relevant plumbing entities
        List<Source> sources = [.. data.Select(d => d.Source).Distinct()];

        Result<List<PlumbingEntity>> plumbs = await _plumbs.FindAsync(p => sources.Contains(p.Source));
        if (plumbs.IsFailure) return Result.Failure<List<ReportPlumbing>>(plumbs.Error);

        HashSet<(PhoneNumber PhoneNumber, long UnixDate, Source Source, string MetaData)> keySet = 
            data.ToHashSetFast(k => (k.PhoneNumber, UnixDate: k.Date.ToUnixTimeSeconds(), k.Source, k.MetaData));
        List<PlumbingEntity> plumbingEntities = [.. plumbs.Value.Where(p => keySet.Contains((p.PhoneNumber, p.UnixDate, p.Source, p.MetaData)))];

        // Get links to the plumbing
        List<long> plumbingIds = [.. plumbingEntities.Select(e => e.Id)];
        Result<List<SandPlumbingLink>> linkResult = await _repo.FindWithDetailsAsync(l => plumbingIds.Contains(l.PlumbingId));
        if (linkResult.IsFailure)
            return Result.Failure<List<ReportPlumbing>>(linkResult.Error);
        List<SandPlumbingLink> links = linkResult.Value;

        // Turn sandplumbinglinks into a hashset of plumbingids for fast lookup
        HashSet<long> ids = links.ToHashSetFast(e => e.PlumbingId);

        // Turn empty PlumbingEntities into sandplumbinglinks
        List<SandPlumbingLink> unfoundPlumbing =
            [.. plumbingEntities
                .Where(e => !ids.Contains(e.Id)) // We are creating a partition between linked plumbing entities and unlinked plumbing entities
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
