using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Data.Transform;

public abstract class TransformPlumbingGeneric<TReport>(
    ISubsPlumbingLinkRepository repo,
    IVoToEntity<Plumbing, PlumbingEntity> voToEntity
    ) : ITransform<Plumbing, TReport>
{
    private readonly ISubsPlumbingLinkRepository _repo = repo;
    private readonly IVoToEntity<Plumbing, PlumbingEntity> _voToEntity = voToEntity;
    public async Task<Result<List<TReport>>> TransformAsync(List<Plumbing> data)
    {
        List<PlumbingEntity> e = [.. data.Select(_voToEntity.Translate)];
        Result<List<SubsPlumbingLink>> links = await _repo.GetAllAsync(e);
        List<SubsPlumbingLink>? entities = links.IsSuccess
            ? links.Value
            : null;
        if (entities is null)
            return Result.Failure<List<TReport>>(links.Error);

        return entities.Select(TransformLink).ToList();
    }
    public abstract TReport TransformLink(SubsPlumbingLink link);
}