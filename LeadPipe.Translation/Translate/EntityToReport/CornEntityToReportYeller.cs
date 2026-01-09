using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.EntityToReport;

internal sealed class CornEntityToReportYeller : IEntityToReport<CornEntity, ReportYeller>
{
    public ReportYeller Translate(CornEntity data)
    {
        long eventTime = data.UnixDate;
        string eventName = "lead";
        string num = YellerReportHelper.HashSha256(data.PhoneNumber.ToString());
        string eventId = data.Id.ToString();

        UserData user = new() { ph = [num] };
        CustomData custom = new()
        {
            currency = YellerReportHelper.Currency,
            value = 0
        };

        ReportYeller result = new()
        {
            event_id = eventId,
            event_name = eventName,
            event_time = eventTime,
            custom_data = custom,
            user_data = user
        };
        return result;
    }
}