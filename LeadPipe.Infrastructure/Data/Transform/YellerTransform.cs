using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Data.Transform;

internal class YellerTransform(
    ISubsPlumbingLinkRepository spRepo,
    IVoToEntity<Plumbing, PlumbingEntity> toEntity
    ) : ITransform<Plumbing, YellerReport>
{
    private readonly ISubsPlumbingLinkRepository _spRepo = spRepo;
    private readonly IVoToEntity<Plumbing, PlumbingEntity> _toEntity = toEntity;
    public async Task<Result<List<YellerReport>>> TransformAsync(List<Plumbing> data)
    {
        List<PlumbingEntity> e = [.. data.Select(_toEntity.Translate)];
        Result<List<SubsPlumbingLink>> eResult = await _spRepo.GetAllAsync(e);
        List<SubsPlumbingLink>? entities = eResult.IsSuccess
            ? eResult.Value
            : null;
        if (entities is null)
            return Result.Failure<List<YellerReport>>(eResult.Error);

        return entities.Select(Transform).ToList();
    }
    const string currency = "USD";
    const string country = "us";
    internal static YellerReport Transform(SubsPlumbingLink data)
    {
        long eventTime = data.SubsEntity.UnixDate;
        UserData user = new() { ph = [data.SubsEntity.Number.ToString(), data.SubsEntity.Number2.ToString()], country = [country] };
        CustomData custom = new() { currency = currency, value = (decimal)data.SubsEntity.Value };
        string eventid = data.SubsEntity.Id.ToString();

        return new() { event_id = eventid, event_time = eventTime, custom_data = custom, user_data = user };
    }
}
