using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.VoToEntity;

internal sealed class CustardToCustardMySqlEntity : IVoToEntity<Custard, CustardMySqlEntity>
{
    public CustardMySqlEntity Translate(Custard s)
    {
        DateTime date = TimeZoneInfo.ConvertTime(s.Date, TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time")).DateTime;
        DateTime? dateCancelled = s.DateCancelled is null ? null : TimeZoneInfo.ConvertTime(s.DateCancelled.Value.UtcDateTime, TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));
        CustardMySqlEntity result = new()
        {
            customerID = (int)s.Id,
            status = s.Status ? 1 : 0,
            phone1 = s.Phone1.Number.ToString(),
            phone2 = s.Phone2?.ToString(),
            dateAdded = date,
            dateCancelled = dateCancelled,
        };
        return result;
    }
}