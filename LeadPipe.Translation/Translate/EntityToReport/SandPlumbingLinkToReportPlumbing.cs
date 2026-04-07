using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Translate;
using System.Text.RegularExpressions;

namespace LeadPipe.Translation.Translate.EntityToReport;

internal partial class SandPlumbingLinkToReportPlumbing(IClock clock) : IEntityToReport<SandPlumbingLink, ReportPlumbing>
{
    private readonly IClock _clock = clock;

    private readonly static DateTimeOffset _twentyTwelve = new(2012, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private const string dateFormat = "yyyy-MM-dd HH:mm:ss";
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
                CancelDate = DateTime.MinValue,
                Offerman = string.Empty
            };

        long phoneNumber = plumb.PhoneNumber.Number;

        DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(plumb.UnixDate);

        string contents = plumb.Contents is string c
            ? NewLineRegex().Replace(
                c.Replace(',', ';'), 
                " | ")
            : string.Empty;
        string source = plumb.Source.ToString();
        string metadata = plumb.MetaData;

        long custardId = sand.CustardId;
        long sandId = sand.Id;
        bool sandActive = sand.Active;
        bool completed = sand.Complete;

        // Dates
        DateTimeOffset? custDate = 
            _twentyTwelve.ToUnixTimeSeconds() >= sand.UnixDate || _clock.UtcNow.ToUnixTimeSeconds() <= sand.UnixDate
                ? null
                : DateTimeOffset.FromUnixTimeSeconds(sand.UnixDate);

        DateTimeOffset? custCxlDate = 
            _twentyTwelve.ToUnixTimeSeconds() >= sand.UnixCancelDate || _clock.UtcNow.ToUnixTimeSeconds() <= sand.UnixCancelDate
                ? null
                : DateTimeOffset.FromUnixTimeSeconds(sand.UnixCancelDate);

        DateTimeOffset? subDate = custDate;

        DateTimeOffset? subCxlDate = custCxlDate;

        bool msgBeforeCust = date < custDate && date < subDate;
        bool isSale = msgBeforeCust && completed;

        string s1 = sand.Seller == 0 ? _unk : sand.Seller.ToString();
        string s2 = sand.Seller2 == 0 ? string.Empty : $" | {sand.Seller2}";
        string s3 = sand.Seller3 == 0 ? string.Empty : $" | {sand.Seller3}";
        string sellers = $"{s1}{s2}{s3}";

        ReportPlumbing result = new()
        {
            PhoneNumber = phoneNumber,
            Date = date,
            Contents = contents,
            Source = source,
            MsgBC = msgBeforeCust,
            IsS = isSale,
            CustardId = custardId,
            SandActive = sandActive,
            CustardDate = custDate,
            CustardCxlDate = custCxlDate,
            SandId = sandId,
            Completed = completed,
            Value = sand.Value,
            SandDate = subDate,
            SandCxlDate = subCxlDate,
            Sellers = sellers,
            MetaData = metadata,
        };

        return result;
    }

    [GeneratedRegex(@"(\s*(\n\r|\r\n|\n|\r)\s*)+")]
    private static partial Regex NewLineRegex();
}
