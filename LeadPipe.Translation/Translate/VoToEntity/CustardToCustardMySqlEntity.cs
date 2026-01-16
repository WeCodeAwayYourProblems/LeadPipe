using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.VoToEntity;

internal sealed class CustardToCustardMySqlEntity : IVoToEntity<Custard, CustardMySqlEntity>
{
    public CustardMySqlEntity Translate(Custard s)
    {
        DateTime date = TimeZoneInfo.ConvertTime(s.Date, TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time")).DateTime;
        DateTime dateCancelled = TimeZoneInfo.ConvertTime(s.DateCancelled, TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time")).DateTime;
        CustardMySqlEntity result = new()
        {
            customerID = (int)s.Id,
            status = s.Status ? 1 : 0,
            phone1 = s.Phone1.Number.ToString(),
            phone2 = s.Phone2.Number.ToString(),
            dateAdded = date,
            dateCancelled = dateCancelled,
        };
        return result;
    }
}