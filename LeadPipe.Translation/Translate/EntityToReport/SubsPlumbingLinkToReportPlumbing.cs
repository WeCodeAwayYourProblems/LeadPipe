using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.EntityToReport;

internal class SubsPlumbingLinkToReportPlumbing : IEntityToReport<SubsPlumbingLink, ReportPlumbing>
{
    private const string dateFormat = "yyyy-MM-dd HH:mm:ss";
    public ReportPlumbing Translate(SubsPlumbingLink link)
    {
        if (link.PlumbingEntity is not PlumbingEntity plumb)
            throw new Exception("Plumbing entities cannot be null");

        if (link.SubsEntity is not SubsEntity sub)
            sub = new()
            {
                CustomerId = 0,
                Id = 0,
                Active = false,
                Complete = false,
                Date = DateTime.MinValue,
                CancelDate = DateTime.MinValue,
                SubDate = DateTime.MinValue,
                SubCancelDate = DateTime.MinValue
            };

        long phoneNumber = plumb.PhoneNumber;

        DateTime d = DateTime.SpecifyKind(plumb.Date, DateTimeKind.Utc);
        DateTimeOffset date = new(d, TimeSpan.Zero);
        string formattedDate = date.ToString(dateFormat);

        string message = plumb.Contents is string c ? c : string.Empty;
        string source = plumb.Source.ToString();
        string metadata = plumb.MetaData;

        long customerId = sub.CustomerId;
        long subId = sub.Id;
        bool subActive = sub.Active;
        bool completed = sub.Complete;

        // Dates
        DateTime cd = DateTime.SpecifyKind(sub.Date, DateTimeKind.Utc);
        DateTimeOffset custDate = new(cd, TimeSpan.Zero);
        string formattedCustDate = custDate.ToString(dateFormat);

        DateTime cxl = DateTime.SpecifyKind(sub.CancelDate, DateTimeKind.Utc);
        DateTimeOffset custCxlDate = new(cxl, TimeSpan.Zero);

        DateTime sd = DateTime.SpecifyKind(sub.SubDate, DateTimeKind.Utc);
        DateTimeOffset subDate = new(sd, TimeSpan.Zero);
        string formattedSubDate = subDate.ToString(dateFormat);

        DateTime sCxl = DateTime.SpecifyKind(sub.SubCancelDate, DateTimeKind.Utc);
        DateTimeOffset subCxlDate = new(sCxl, TimeSpan.Zero);

        bool msgBeforeCust = date < custDate && date < subDate;
        bool isSale = msgBeforeCust && completed;

        ReportPlumbing result = new()
        {
            MsgBeforeCust = msgBeforeCust,
            IsSale = isSale,
            PhoneNumber = phoneNumber,
            Date = date,
            FormattedDate = formattedDate,
            Message = message,
            Source = source,
            MetaData = metadata,
            CustomerId = customerId,
            SubId = subId,
            SubActive = subActive,
            Completed = completed,
            CustDate = custDate,
            FormattedCustDate = formattedCustDate,
            CustCxlDate = custCxlDate,
            SubDate = subDate,
            FormattedSubDate = formattedSubDate,
            SubCxlDate = subCxlDate,
        };
        return result;
    }
}
