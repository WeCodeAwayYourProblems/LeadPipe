using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.EntityToVo;

internal class CustardMySqlEntityToCustard : IEntityToVo<CustardMySqlEntity, Custard>
{
    public Custard Translate(CustardMySqlEntity entity)
    {
        PhoneNumber phone1 = PhoneNumber.TryParse(entity.phone1, out var p1) ? p1 : new PhoneNumber(PhoneNumber.Default);
        PhoneNumber phone2 = PhoneNumber.TryParse(entity.phone2, out var p2) ? p2 : new PhoneNumber(PhoneNumber.Default);
        bool status = entity.status == 1;

        Custard result = new
        (
            Id: entity.customerID,
            Status: status,
            Phone1: phone1,
            Phone2: phone2,
            Date: entity.dateAdded,
            DateCancelled: entity.dateCancelled
        );
        return result;
    }
}
