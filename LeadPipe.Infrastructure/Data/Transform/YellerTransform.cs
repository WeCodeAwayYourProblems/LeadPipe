using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Data.Transform;

internal sealed class YellerTransform(
    ISubsPlumbingLinkRepository spRepo,
    IVoToEntity<Plumbing, PlumbingEntity> toEntity
    ) : TransformPlumbingGeneric<ReportYeller>(spRepo, toEntity)
{
    const string currency = "USD";
    const string country = "us";
    public override ReportYeller TransformLink(SubsPlumbingLink link)
    {
        long eventTime = link.SubsEntity.UnixDate;
        UserData user = new() { ph = [link.SubsEntity.Number.ToString(), link.SubsEntity.Number2.ToString()], country = [country] };
        CustomData custom = new() { currency = currency, value = link.SubsEntity.Value };
        string eventid = link.SubsEntity.Id.ToString();

        return new() { event_id = eventid, event_time = eventTime, custom_data = custom, user_data = user };
    }
}
