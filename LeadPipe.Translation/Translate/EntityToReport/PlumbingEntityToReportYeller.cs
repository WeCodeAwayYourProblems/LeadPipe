using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Settings;

namespace LeadPipe.Translation.Translate.EntityToReport;

internal sealed class PlumbingEntityToReportYeller(IYellerSettings settings) : EntityToReportYeller<PlumbingEntity>
{
    protected override string Type => settings.YellerPlumbingName!;

    protected override long GetEntityId(PlumbingEntity e) => e.Id;

    protected override long GetPhoneNumber(PlumbingEntity e) => e.PhoneNumber.Number;

    protected override long GetUnixDate(PlumbingEntity e) => e.UnixDate;
}