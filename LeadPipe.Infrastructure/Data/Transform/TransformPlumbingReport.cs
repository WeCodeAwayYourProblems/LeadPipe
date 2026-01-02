using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Data.Transform;

public sealed class TransformPlumbingReport(
    ISubsPlumbingLinkRepository repo,
    IVoToEntity<Plumbing, PlumbingEntity> voToEntity
    ) : ITransform<Plumbing, ReportPlumbing>
{
    private readonly ISubsPlumbingLinkRepository _repo = repo;
    private readonly IVoToEntity<Plumbing, PlumbingEntity> _voToEntity = voToEntity;
    private const string dateFormat = "yyyy-MM-dd HH:mm:ss";
    public async Task<Result<List<ReportPlumbing>>> TransformAsync(List<Plumbing> data)
    {
        List<PlumbingEntity> e = [.. data.Select(_voToEntity.Translate)];
        Result<List<SubsPlumbingLink>> links = await _repo.GetAllWithDetailsAsync(e);
        List<SubsPlumbingLink>? entities = links.IsSuccess
            ? links.Value
            : null;
        if (entities is null)
            return Result.Failure<List<ReportPlumbing>>(links.Error);

        return entities.Select(TransformLink).ToList();
    }
    private static ReportPlumbing TransformLink(SubsPlumbingLink link)
    {
        long phoneNumber = link.PlumbingEntity!.PhoneNumber;

        DateTime d = DateTime.SpecifyKind(link.PlumbingEntity.Date, DateTimeKind.Utc);
        DateTimeOffset date = new(d, TimeSpan.Zero);
        string formattedDate = date.ToString(dateFormat);

        string message = link.PlumbingEntity.Contents is string c ? c : string.Empty;
        string source = link.PlumbingEntity.Source.ToString();
        string metadata = link.PlumbingEntity.MetaData;

        long customerId = link.SubsEntity!.CustomerId;
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
