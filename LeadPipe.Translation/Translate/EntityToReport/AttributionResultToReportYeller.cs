using LeadPipe.Core;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Infrastructure.Settings;

namespace LeadPipe.Translation.Translate.EntityToReport;

internal sealed class AttributionResultToReportYeller(IYellerSettings settings) : IEntityToReport<AttributionResult, ReportYeller>
{
    private readonly string _corn = settings.YellerCornName!;
    private readonly string _plumbing = settings.YellerPlumbingName!;
    private readonly string _caliper = settings.YellerCaliperName!;
    private const string _unk = "Unknown";
    public ReportYeller Translate(AttributionResult attr)
    {
        string medium = attr.Source switch
        {
            AttributionSource.Plumbing => _plumbing,
            AttributionSource.Caliper => _caliper,
            AttributionSource.Corn => _corn,
            _ => _unk
        };

        string s1 = attr.Sand.Seller == 0 ? _unk : attr.Sand.Seller.ToString();
        string s2 = attr.Sand.Seller2 == 0 ? string.Empty : $" | {attr.Sand.Seller2}";
        string s3 = attr.Sand.Seller3 == 0 ? string.Empty : $" | {attr.Sand.Seller3}";
        string sellers = $"{s1}{s2}{s3}";

        long phone = attr.MatchingPhone;

        DateTime eventDate = DateTimeOffsetExt.FromUnixTime(attr.Entity.UnixDate).UtcDateTime;
        long unixCloseDate = attr.Custard.UnixDate < attr.Sand.UnixDate ? attr.Custard.UnixDate : attr.Sand.UnixDate;
        DateTime closeDate = DateTimeOffsetExt.FromUnixTime(unixCloseDate).UtcDateTime;

        var result = new ReportYeller()
        {
            EventMedium = medium,
            PhoneNumber = phone,
            EventDateUtc = eventDate,
            CloseDateUtc = closeDate,
            Value = attr.Value,
            Type = attr.Sand.Type is null ? _unk : attr.Sand.Type,
            Completed = attr.Sand.Complete,
            CombinedSellers = sellers,
            SId = attr.Sand.Id,
            EId = attr.Entity.Id,
        };

        return result;
    }

}