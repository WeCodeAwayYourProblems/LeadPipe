using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Infrastructure.Settings;

namespace LeadPipe.Translation.Translate.EntityToReport;

internal sealed class CaliperEntityToReportYeller(IYellerSettings settings) : IEntityToReport<CaliperEntity, ReportYeller>
{
    private readonly string _action = settings.YellerActionSource!;
    public ReportYeller Translate(CaliperEntity data)
    {
        long eventtime = data.UnixDate;
        string eventname = "lead";
        string num = YellerReportHelper.HashSha256(data.PhoneNumber.Number.ToString());
        string eventid = data.Id.ToString();

        UserData user = new() { ph = [num] };
        CustomData custom = new()
        {
            currency = YellerReportHelper.Currency,
            value = 0
        };
        ReportYeller result = new()
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
