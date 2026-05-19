using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace LeadPipe.Infrastructure.Dto;

public class ReportFilePlumbingMap : ClassMap<ReportPlumbing_Old>
{
    public ReportFilePlumbingMap()
    {
        int index = 0;
        Map(m => m.PhoneNumber).Index(index++).Name(ReportPlumbing_Old.PhoneNumberName);
        Map(m => m.Date).Index(index++).Name(ReportPlumbing_Old.DateName);
        Map(m => m.FormattedDate).Index(index++).Name(ReportPlumbing_Old.FormattedDateName);
        Map(m => m.Message).Index(index++).Name(ReportPlumbing_Old.MessageName);
        Map(m => m.Source).Index(index++).Name(ReportPlumbing_Old.SourceName);
        Map(m => m.MetaData).Index(index++).Name(ReportPlumbing_Old.MetaDataName);
        Map(m => m.MsgBeforeCust).Index(index++).Name(ReportPlumbing_Old.MsgBeforeCustName);
        Map(m => m.IsSale).Index(index++).Name(ReportPlumbing_Old.IsSaleName);
        Map(m => m.CustomerId).Index(index++).Name(ReportPlumbing_Old.CustomerIdName);
        Map(m => m.SubId).Index(index++).Name(ReportPlumbing_Old.SubIdName);
        Map(m => m.SubActive).Index(index++).Name(ReportPlumbing_Old.SubActiveName);
        Map(m => m.Completed).Index(index++).Name(ReportPlumbing_Old.CompletedName);
        Map(m => m.CustDate).Index(index++).Name(ReportPlumbing_Old.CustDateName);
        Map(m => m.FormattedCustDate).Index(index++).Name(ReportPlumbing_Old.FormattedCustDateName);
        Map(m => m.CustCxlDate).Index(index++).Name(ReportPlumbing_Old.CustCxlDateName);
        Map(m => m.SubDate).Index(index++).Name(ReportPlumbing_Old.SubDateName);
        Map(m => m.FormattedSubDate).Index(index++).Name(ReportPlumbing_Old.FormattedSubDateName);
        Map(m => m.SubCxlDate).Index(index++).Name(ReportPlumbing_Old.SubCxlDateName);
    }
}
public class ReportPlumbing_Old
{
    [Name(PhoneNumberName)]
    public long PhoneNumber { get; set; }
    public const string PhoneNumberName = "Phone Number";

    [Name(DateName)]
    public DateTimeOffset Date { get; set; }
    public const string DateName = "Date";

    [Name(FormattedDateName)]
    public required string FormattedDate { get; set; }
    public const string FormattedDateName = "Formatted Date";

    [Name(MessageName)]
    public required string Message { get; set; }
    public const string MessageName = "Message";

    [Name(SourceName)]
    /// <summary>
    /// This source denotes where the source came from, which can be domain or something outside the application and may be relatively arbitrary
    /// </summary>
    public required string Source { get; set; }
    public const string SourceName = "Source";

    [Name(MetaDataName)]
    public required string MetaData { get; set; }
    public const string MetaDataName = "Meta Data";

    [Name(MsgBeforeCustName)]
    public bool MsgBeforeCust { get; set; }
    public const string MsgBeforeCustName = "Msg Before Cust";

    [Name(IsSaleName)]
    public bool IsSale { get; set; }
    public const string IsSaleName = "Is a Sale";

    [Name(CustomerIdName)]
    public long CustomerId { get; set; }
    public const string CustomerIdName = "Customer Id";

    [Name(SubIdName)]
    public long SubId { get; set; }
    public const string SubIdName = "Sub Id";

    [Name(SubActiveName)]
    public bool SubActive { get; set; }
    public const string SubActiveName = "Sub is Active";

    [Name(CompletedName)]
    public bool Completed { get; set; }
    public const string CompletedName = "Completed";

    [Name(CustDateName)]
    public DateTimeOffset CustDate { get; set; }
    public const string CustDateName = "Cust Date";

    [Name(FormattedCustDateName)]
    public required string FormattedCustDate { get; set; }
    public const string FormattedCustDateName = "Formatted Cust Date";

    [Name(CustCxlDateName)]
    public DateTimeOffset CustCxlDate { get; set; }
    public const string CustCxlDateName = "Cust Cxl Date";

    [Name(SubDateName)]
    public DateTimeOffset SubDate { get; set; }
    public const string SubDateName = "Sub Date";

    [Name(FormattedSubDateName)]
    public required string FormattedSubDate { get; set; }
    public const string FormattedSubDateName = "Formatted Sub Date";

    [Name(SubCxlDateName)]
    public DateTimeOffset SubCxlDate { get; set; }
    public const string SubCxlDateName = "Sub Cxl Date";
}
public sealed class ReportPlumbing
{
    [Name(ReportPlumbingColumnNames.PhoneNumberName)]
    public long PhoneNumber { get; set; }
    [Name(ReportPlumbingColumnNames.DateName)]
    public DateTime DateUtc { get; set; }
    [Name(ReportPlumbingColumnNames.ContentsName)]
    public required string Contents { get; set; }
    [Name(ReportPlumbingColumnNames.SourceName)]
    public required string Source { get; set; }
    [Name(ReportPlumbingColumnNames.MsgBCName)]
    public bool? MsgBC { get; set; }
    [Name(ReportPlumbingColumnNames.IsSName)]
    public bool? IsS { get; set; }
    [Name(ReportPlumbingColumnNames.CustardIdName)]
    public long? CustardId { get; set; }
    [Name(ReportPlumbingColumnNames.SandActiveName)]
    public bool? SandActive { get; set; }
    [Name(ReportPlumbingColumnNames.CustardDateName)]
    public DateTime? CustardDateUtc { get; set; }
    [Name(ReportPlumbingColumnNames.CustardCxlDateName)]
    public DateTime? CustardCxlDateUtc { get; set; }
    [Name(ReportPlumbingColumnNames.SandIdName)]
    public long? SandId { get; set; }
    [Name(ReportPlumbingColumnNames.CompletedName)]
    public bool? Completed { get; set; }
    [Name(ReportPlumbingColumnNames.ValueName)]
    public decimal? Value { get; set; }
    [Name(ReportPlumbingColumnNames.SandDateName)]
    public DateTime? SandDateUtc { get; set; }
    [Name(ReportPlumbingColumnNames.SandCxlDateName)]
    public DateTime? SandCxlDateUtc { get; set; }
    [Name(ReportPlumbingColumnNames.SellersName)]
    public string? Sellers { get; set; }
    [Name(ReportPlumbingColumnNames.MetaDataName)]
    public string? MetaData { get; set; }
}
public sealed class ReportPlumbingColumnNames

{
    public const string PhoneNumberName = "Phone Number";
    public const string DateName = "Date";
    public const string ContentsName = "Contents";
    public const string SourceName = "Source";
    public const string MsgBCName = "MsgBC";
    public const string IsSName = "IsS";
    public const string CustardIdName = "Custard Id";
    public const string SandActiveName = "Sand Active";
    public const string CustardDateName = "Custard Date";
    public const string CustardCxlDateName = "Custard Cxl Date";
    public const string SandIdName = "Sand Id";
    public const string CompletedName = "Completed";
    public const string ValueName = "Value";
    public const string SandDateName = "Sand Date";
    public const string SandCxlDateName = "Sand Cxl Date";
    public const string SellersName = "Sellers";
    public const string MetaDataName = "MetaData";
}