using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Infrastructure.Settings;

namespace LeadPipe.Translation.Translate.EntityToReport;

internal sealed class CaliperEntityToReportYellerOld(IYellerSettings settings) : IEntityToReport<CaliperEntity, ReportYeller_Old>
{
    private readonly string _action = settings.YellerActionSource!;
    public ReportYeller_Old Translate(CaliperEntity data)
    {
        long eventtime = data.UnixDate;
        string eventname = "lead";
        string num = YellerReportHelper.HashSha256(data.PhoneNumber.Number.ToString());
        string eventid = data.Id.ToString() + "-3112"; // 3 == c, 1 == a, 12 == l

        UserData user = new() { ph = [num] };
        CustomData custom = new()
        {
            currency = YellerReportHelper.Currency,
            value = 0
        };
        ReportYeller_Old result = new()
        {
            event_id = eventid,
            event_name = eventname,
            event_time = eventtime,
            custom_data = custom,
            action_source = _action,
            user_data = user
        };
        return result;
    }
}
