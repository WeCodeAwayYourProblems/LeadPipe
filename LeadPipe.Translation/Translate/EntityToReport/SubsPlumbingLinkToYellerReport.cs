using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.EntityToReport;

internal sealed class SubsPlumbingLinkToYellerReport : IEntityToReport<SubsPlumbingLink, ReportYeller>
{
    private const string currency = "USD";
    private const string country = "us";
    public ReportYeller Translate(SubsPlumbingLink link)
    {
        long eventTime = link.SubsEntity.UnixDate;
        UserData user = new()
        {
            ph = [link.SubsEntity.Number.ToString(), link.SubsEntity.Number2.ToString()],
            country = [country]
        };
        CustomData custom = new()
        {
            currency = currency,
            value = link.SubsEntity.Value
        };
        string eventid = link.SubsEntity.Id.ToString();

        return new() { event_id = eventid, event_time = eventTime, custom_data = custom, user_data = user };
    }
}