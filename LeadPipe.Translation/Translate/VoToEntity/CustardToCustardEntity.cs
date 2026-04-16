using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.VoToEntity;

internal sealed class CustardToCustardEntity : IVoToEntity<Custard, CustardEntity>
{
    public CustardEntity Translate(Custard s)
    {
        CustardEntity result = new()
        {
            Id = s.Id,
            Active = s.Status,
            PhoneNumber = s.Phone1,
            PhoneNumber2 = s.Phone2,
            Date = s.Date.UtcDateTime,
            UnixDate = s.Date.ToUnixTimeSeconds(),
            UnixCancelDate = s.DateCancelled?.ToUnixTimeSeconds(),
        };
        return result;
    }
}
