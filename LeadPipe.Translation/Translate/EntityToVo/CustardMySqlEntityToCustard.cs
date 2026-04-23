using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Translation.Primitives;

namespace LeadPipe.Translation.Translate.EntityToVo;

internal class CustardMySqlEntityToCustard(IDateTimeTranslate dt) : IEntityToVo<CustardMySqlEntity, Custard>
{
    private readonly IDateTimeTranslate _dt = dt;
    public Custard Translate(CustardMySqlEntity entity)
    {
        PhoneNumber phone1 = PhoneNumber.TryParse(entity.phone1, out var p1) ? p1 : new PhoneNumber(PhoneNumber.Default);
        PhoneNumber phone2 = PhoneNumber.TryParse(entity.phone2, out var p2) ? p2 : new PhoneNumber(PhoneNumber.Default);

        DateTime d = entity.dateAdded is DateTime added
            ? added : DateTime.MinValue;
        DateTime dcxl = entity.dateCancelled is DateTime cxl
            ? cxl : DateTime.MinValue;
        DateTimeOffset date = _dt.Convert(d, ETimeZone.Pacific);
        DateTimeOffset dateCancelled = _dt.Convert(dcxl, ETimeZone.Pacific);

        bool status = entity.status == 1;

        Custard result = new
        (
            Id: entity.customerID,
            Status: status,
            Phone1: phone1,
            Phone2: phone2,
            Date: date,
            DateCancelled: dateCancelled
        );
        return result;
    }
}
