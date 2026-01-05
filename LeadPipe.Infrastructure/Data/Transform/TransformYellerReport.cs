using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Data.Transform;

internal sealed class TransformYellerReport(
    ISubsPlumbingLinkRepository spRepo,
    IVoToEntity<Plumbing, PlumbingEntity> toEntity,
    IEntityToReport<SubsPlumbingLink, ReportYeller> eToR
    ) : ITransform<Plumbing, ReportYeller>
{
    private readonly ISubsPlumbingLinkRepository _repo = spRepo;
    private readonly IVoToEntity<Plumbing, PlumbingEntity> _voToEntity = toEntity;
    private readonly IEntityToReport<SubsPlumbingLink, ReportYeller> _eToR = eToR;
    
    public async Task<Result<List<ReportYeller>>> TransformAsync(List<Plumbing> data)
    {
        List<PlumbingEntity> e = [.. data.Select(_voToEntity.Translate)];
        Result<List<SubsPlumbingLink>> links = await _repo.GetAllAsync(e);
        List<SubsPlumbingLink>? entities = links.IsSuccess
            ? links.Value
            : null;
        if (entities is null)
            return Result.Failure<List<ReportYeller>>(links.Error);

        return entities.Select(_eToR.Translate).ToList();
    }

}
