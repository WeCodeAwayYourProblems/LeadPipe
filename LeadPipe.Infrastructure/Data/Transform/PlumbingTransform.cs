using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Data.Transform;

public sealed class PlumbingTransform(
    ISubsPlumbingLinkRepository repo,
    IVoToEntity<Plumbing, PlumbingEntity> voToEntity
    ) : TransformPlumbingGeneric<ReportFilePlumbing>(repo, voToEntity)
{
    private const string dateFormat = "yyyy-MM-dd HH:mm:ss";
    protected override ReportFilePlumbing TransformLink(SubsPlumbingLink link)
    {
        long phoneNumber = link.PlumbingEntity.PhoneNumber;

        DateTime d = DateTime.SpecifyKind(link.PlumbingEntity.Date, DateTimeKind.Utc);
        DateTimeOffset date = new(d, TimeSpan.Zero);
        string formattedDate = date.ToString(dateFormat);

        string message = link.PlumbingEntity.Contents is string c ? c : string.Empty;
        string source = link.PlumbingEntity.Source.ToString();
        string metadata = link.PlumbingEntity.MetaData;

        long customerId = link.SubsEntity.CustomerId;
        long subId = link.SubsEntity.Id;
        bool subActive = link.SubsEntity.Active;
        bool completed = link.SubsEntity.Complete;

        // Dates
        DateTime cd = DateTime.SpecifyKind(link.SubsEntity.Date, DateTimeKind.Utc);
        DateTimeOffset custDate = new(cd, TimeSpan.Zero);
        string formattedCustDate = custDate.ToString(dateFormat);

        DateTime cxl = DateTime.SpecifyKind(link.SubsEntity.CancelDate, DateTimeKind.Utc);
        DateTimeOffset custCxlDate = new(cxl, TimeSpan.Zero);

        DateTime sd = DateTime.SpecifyKind(link.SubsEntity.SubDate, DateTimeKind.Utc);
        DateTimeOffset subDate = new(sd, TimeSpan.Zero);
        string formattedSubDate = subDate.ToString(dateFormat);

        DateTime sCxl = DateTime.SpecifyKind(link.SubsEntity.SubCancelDate, DateTimeKind.Utc);
        DateTimeOffset subCxlDate = new(sCxl, TimeSpan.Zero);

        bool msgBeforeCust = date < custDate && date < subDate;
        bool isSale = msgBeforeCust && completed;

        ReportFilePlumbing result = new()
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
