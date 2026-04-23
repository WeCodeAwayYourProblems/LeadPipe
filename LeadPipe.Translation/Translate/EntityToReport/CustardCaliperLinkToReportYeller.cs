using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Infrastructure.Settings;

namespace LeadPipe.Translation.Translate.EntityToReport;

internal sealed class CustardCaliperLinkToReportYellerOld(IYellerSettings settings) : IEntityToReport<CustardCaliperLink, ReportYeller_Old>
{
    private readonly string _action = settings.YellerActionSource!;
    public ReportYeller_Old Translate(CustardCaliperLink data)
    {
        if (data.Custard is null)
            throw new ArgumentNullException($"{nameof(data)} has null navigation properties that cannot be null", new Exception($"Navigation properties in {nameof(CustardCaliperLink)} cannot be null"));

        long eventtime = data.UnixMatchDate;
        string eventname = "purchase";
        string eventid = data.Custard.Id.ToString();

        string num1 = YellerReportHelper.HashSha256(data.Custard.PhoneNumber.Number.ToString());
        string n2 = data.Custard.PhoneNumber2 is null
            ? PhoneNumber.Default.ToString()
            : data.Custard.PhoneNumber2.Number.ToString();
        string num2 = YellerReportHelper.HashSha256(n2);

        UserData user = new() { ph = [num1, num2] };
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
