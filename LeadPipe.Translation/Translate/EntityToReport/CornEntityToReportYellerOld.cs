using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Infrastructure.Settings;

namespace LeadPipe.Translation.Translate.EntityToReport;

internal sealed class CornEntityToReportYellerOld(IYellerSettings settings) : IEntityToReport<CornEntity, ReportYeller_Old>
{
    private readonly string _action = settings.YellerActionSource!;
    public ReportYeller_Old Translate(CornEntity data)
    {
        long eventTime = data.UnixDate;
        string eventName = "lead";
        string num = YellerReportHelper.HashSha256(data.PhoneNumber.Number.ToString());
        string eventId = data.Id.ToString() + "-31518"; // 3 == c, 15 == o, 18 == r

        UserData user = new() { ph = [num] };
        CustomData custom = new()
        {
            currency = YellerReportHelper.Currency,
            value = 0
        };

        ReportYeller_Old result = new()
        {
            event_id = eventId,
            event_name = eventName,
            event_time = eventTime,
            custom_data = custom,
            action_source = _action,
            user_data = user
        };
        return result;
    }
}
