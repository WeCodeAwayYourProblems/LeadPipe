using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.EntityToReport;

internal sealed class SandEntityToReportYeller : IEntityToReport<SandEntity, ReportYeller>
{
    public ReportYeller Translate(SandEntity sub)
    {
        long eventTime = sub.UnixDate;
        string eventName = "purchase";

        // Hash pii
        string num1 = YellerReportHelper.HashSha256(sub.PhoneNumber.ToString());
        string num2 = YellerReportHelper.HashSha256(sub.PhoneNumber2.ToString());

        UserData user = new()
        {
            ph = [num1, num2],
        };
        CustomData custom = new()
        {
            currency = YellerReportHelper.Currency,
            value = sub.Value
        };
        string eventid = sub.Id.ToString();

        ReportYeller result = new()
        {
            event_id = eventid,
            event_name = eventName,
            event_time = eventTime,
            custom_data = custom,
            user_data = user
        };
        return result;
    }
}
