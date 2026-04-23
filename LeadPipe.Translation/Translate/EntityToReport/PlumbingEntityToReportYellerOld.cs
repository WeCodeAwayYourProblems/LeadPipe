using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Infrastructure.Settings;

namespace LeadPipe.Translation.Translate.EntityToReport;

internal sealed class PlumbingEntityToReportYellerOld(IYellerSettings settings) : IEntityToReport<PlumbingEntity, ReportYeller_Old>
{
    private readonly string _action = settings.YellerActionSource!;
    public ReportYeller_Old Translate(PlumbingEntity data)
    {
        long eventtime = data.UnixDate;
        string eventName = "lead";
        string num = YellerReportHelper.HashSha256(data.PhoneNumber.Number.ToString());
        string eventid = data.Id.ToString() + "-161221"; // 16 == p, 12 == l, 21 == u

        UserData user = new() { ph = [num] };
        CustomData custom = new()
        {
            currency = YellerReportHelper.Currency,
            value = 0
        };

        ReportYeller_Old result = new()
        {
            event_id = eventid,
            event_name = eventName,
            event_time = eventtime,
            action_source = _action,
            custom_data = custom,
            user_data = user
        };
        return result;
    }
}
