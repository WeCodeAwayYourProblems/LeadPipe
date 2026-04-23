using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Settings;

namespace LeadPipe.Translation.Translate.EntityToReport;
internal sealed class CornEntityToReportYeller(IYellerSettings settings) : EntityToReportYeller<CornEntity>
{
    protected override string Type => settings.YellerCornName!;

    protected override long GetEntityId(CornEntity e) => e.Id;

    protected override long GetPhoneNumber(CornEntity e) => e.PhoneNumber.Number;

    protected override long GetUnixDate(CornEntity e) => e.UnixDate;
}
