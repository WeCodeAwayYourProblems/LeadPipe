using LeadPipe.Core;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Translate;
using System.Text.RegularExpressions;

namespace LeadPipe.Translation.Translate.EntityToReport;

internal partial class SandPlumbingLinkToReportPlumbing(IClock clock) : IEntityToReport<SandPlumbingLink, ReportPlumbing>
{
    private readonly IClock _clock = clock;

    private readonly static DateTimeOffset _twentyTwelve = new(2012, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private const string _unk = "Unknown";
    public ReportPlumbing Translate(SandPlumbingLink t)
    {
        if (t.PlumbingEntity is not PlumbingEntity plumb)
            throw new Exception("Plumbing entities cannot be null");

        if (t.SandEntity is not SandEntity sand)
            sand = new()
            {
                CustardId = 0,
                Id = 0,
                Active = false,
                Complete = false,
                Date = DateTime.MinValue,
                UnixCancelDate = null,
                Offerman = string.Empty
            };

        long phoneNumber = plumb.PhoneNumber.Number;

        DateTime date = DateTimeOffsetExt.FromUnixTime(plumb.UnixDate).UtcDateTime;

        string contents = plumb.Contents is string c
            ? NewLineRegex().Replace(
                c.Replace(',', ';'),
                " | ")
            : string.Empty;
        string source = plumb.Source.ToString();
        string metadata = plumb.MetaData;

        long? custardId = sand.CustardId == 0 ? null : sand.CustardId;
        long? sandId = sand.Id == 0 ? null : sand.Id;
        bool? sandActive = sandId is null ? null : sand.Active;

        // Dates
        DateTime? custDate =
            _twentyTwelve.ToUnixTime() >= sand.UnixDate || _clock.UtcNow.ToUnixTime() <= sand.UnixDate
                ? null
                : DateTimeOffsetExt.FromUnixTime(sand.UnixDate).UtcDateTime;

        DateTime? custCxlDate =
            sand.UnixCancelDate is null || _twentyTwelve.ToUnixTime() >= sand.UnixCancelDate || _clock.UtcNow.ToUnixTime() <= sand.UnixCancelDate
                ? null
                : DateTimeOffsetExt.FromUnixTime(sand.UnixCancelDate.Value).UtcDateTime;

        DateTime? subDate = custDate;

        DateTime? subCxlDate = custCxlDate;

        bool msgBeforeCust =
            custDate is not null &&
            subDate is not null &&
            date < custDate &&
            date < subDate;
        bool isSale = msgBeforeCust && sand.Complete;

        string s1 = sand.Seller == 0 ? string.Empty : sand.Seller.ToString();
        string s2 = sand.Seller2 == 0 ? string.Empty : $" | {sand.Seller2}";
        string s3 = sand.Seller3 == 0 ? string.Empty : $" | {sand.Seller3}";
        string sellers = $"{s1}{s2}{s3}";

        ReportPlumbing result = new()
        {
            PhoneNumber = phoneNumber,
            DateUtc = date,
            Contents = contents,
            Source = source,
            MsgBC = msgBeforeCust,
            IsS = isSale,
            CustardId = custardId,
            SandActive = sandActive,
            CustardDateUtc = custDate,
            CustardCxlDateUtc = custCxlDate,
            SandId = sandId,
            Completed = sandId is null ? null : sand.Complete,
            Value = sandId is null ? null : sand.Value,
            SandDateUtc = subDate,
            SandCxlDateUtc = subCxlDate,
            Sellers = string.IsNullOrWhiteSpace(sellers) ? null : sellers,
            MetaData = metadata,
        };

        return result;
    }

    [GeneratedRegex(@"(\s*(\n\r|\r\n|\n|\r)\s*)+")]
    private static partial Regex NewLineRegex();
}
