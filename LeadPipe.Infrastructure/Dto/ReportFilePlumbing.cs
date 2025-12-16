using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using System;

namespace LeadPipe.Infrastructure.Dto;

public class ReportFilePlumbingMap : ClassMap<ReportFilePlumbing>
{
    public ReportFilePlumbingMap()
    {
        int index = 0;
        Map(m => m.PhoneNumber).Index(index++).Name(ReportFilePlumbing.PhoneNumberName);
        Map(m => m.Date).Index(index++).Name(ReportFilePlumbing.DateName);
        Map(m => m.FormattedDate).Index(index++).Name(ReportFilePlumbing.FormattedDateName);
        Map(m => m.Message).Index(index++).Name(ReportFilePlumbing.MessageName);
        Map(m => m.Source).Index(index++).Name(ReportFilePlumbing.SourceName);
        Map(m => m.MetaData).Index(index++).Name(ReportFilePlumbing.MetaDataName);
        Map(m => m.MsgBeforeCust).Index(index++).Name(ReportFilePlumbing.MsgBeforeCustName);
        Map(m => m.IsSale).Index(index++).Name(ReportFilePlumbing.IsSaleName);
        Map(m => m.CustomerId).Index(index++).Name(ReportFilePlumbing.CustomerIdName);
        Map(m => m.SubId).Index(index++).Name(ReportFilePlumbing.SubIdName);
        Map(m => m.SubActive).Index(index++).Name(ReportFilePlumbing.SubActiveName);
        Map(m => m.Completed).Index(index++).Name(ReportFilePlumbing.CompletedName);
        Map(m => m.CustDate).Index(index++).Name(ReportFilePlumbing.CustDateName);
        Map(m => m.FormattedCustDate).Index(index++).Name(ReportFilePlumbing.FormattedCustDateName);
        Map(m => m.CustCxlDate).Index(index++).Name(ReportFilePlumbing.CustCxlDateName);
        Map(m => m.SubDate).Index(index++).Name(ReportFilePlumbing.SubDateName);
        Map(m => m.FormattedSubDate).Index(index++).Name(ReportFilePlumbing.FormattedSubDateName);
        Map(m => m.SubCxlDate).Index(index++).Name(ReportFilePlumbing.SubCxlDateName);
    }
}
public class ReportFilePlumbing
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
