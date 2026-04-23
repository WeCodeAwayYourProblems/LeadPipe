using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Infrastructure.Settings;

namespace LeadPipe.Translation.Translate.EntityToReport;

internal sealed class SandEntityToReportYellerOld(IYellerSettings settings) : IEntityToReport<SandEntity, ReportYeller_Old>
{
    private readonly string _action = settings.YellerActionSource!;
    public ReportYeller_Old Translate(SandEntity sub)
    {
        if(sub.CustardEntity is null)
            throw new ArgumentNullException(nameof(sub),$"{nameof(SandEntity)} cannot have a null {nameof(CustardEntity)}");

        long eventTime = sub.UnixDate;
        string eventName = "purchase";

        // Hash pii
        var num2NullCheck = sub.CustardEntity.PhoneNumber2 is null ? PhoneNumber.Default.ToString() : sub.CustardEntity.PhoneNumber2.Number.ToString();
        string num1 = YellerReportHelper.HashSha256(sub.CustardEntity.PhoneNumber.Number.ToString());
        string num2 = YellerReportHelper.HashSha256(num2NullCheck);

        UserData user = new()
        {
            ph = [num1, num2],
        };
        CustomData custom = new()
        {
            currency = YellerReportHelper.Currency,
            value = sub.Value
        };
        string eventid = sub.Id.ToString() + "-19114"; // 19 == s, 1 == a, 14 == n

        ReportYeller_Old result = new()
        {
            event_id = eventid,
            event_name = eventName,
            event_time = eventTime,
            custom_data = custom,
            action_source = _action,
            user_data = user
        };
        return result;
    }
}
