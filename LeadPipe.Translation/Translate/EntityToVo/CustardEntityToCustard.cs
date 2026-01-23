using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.EntityToVo;

internal sealed class CustardEntityToCustard : IEntityToVo<CustardEntity, Custard>
{
    public Custard Translate(CustardEntity entity)
    {
        PhoneNumber number1 = new(entity.PhoneNumber);
        PhoneNumber number2 = new(entity.PhoneNumber);
        
        DateTime d = DateTime.SpecifyKind(entity.Date, DateTimeKind.Utc);
        DateTimeOffset date = new(d, TimeSpan.Zero);
        
        DateTime cxl = DateTime.SpecifyKind(entity.CancelDate, DateTimeKind.Utc);
        DateTimeOffset cxlDate = new(cxl, TimeSpan.Zero);
        
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