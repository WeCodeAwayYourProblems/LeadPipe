using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.EntityToVo;

internal sealed class CustardEntityToCustard : IEntityToVo<CustardEntity, Custard>
{
    public Custard Translate(CustardEntity entity)
    {
        PhoneNumber number1 = new(entity.PhoneNumber);
        PhoneNumber? number2 = entity.PhoneNumber2 is not null ? new(entity.PhoneNumber2) : null;

        DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(entity.UnixDate);

        DateTimeOffset? cxlDate = entity.UnixCancelDate is null ? null : DateTimeOffset.FromUnixTimeSeconds((long)entity.UnixCancelDate);

        Custard result = new
            (
                Id: entity.Id,
                Status: entity.Active,
                Phone1: number1,
                Phone2: number2,
                Date: date,
                DateCancelled: cxlDate
            );
        return result;
    }
}