using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Translation.Primitives;

namespace LeadPipe.Translation.Translate.EntityToVo;

internal sealed class SandMySqlEntityToSandwich(IDateTimeTranslate dt) : IEntityToVo<SandMySqlEntity, Sandwich>
{
    private readonly IDateTimeTranslate _dt = dt;

    public Sandwich Translate(SandMySqlEntity entity)
    {
        if (entity.customer is null)
            throw new ArgumentException($"{nameof(entity.customer)} cannot be null.");

        // Convert navigation property
        DateTimeOffset ceDate = _dt.Convert(entity.customer.dateAdded, ETimeZone.Pacific);
        DateTimeOffset ceDateCancelled = _dt.Convert(entity.customer.dateCancelled, ETimeZone.Pacific);
        Custard ce = new
        (
            Id: entity.customerID,
            Status: entity.customer.status == 1,
            Phone1: PhoneNumber.TryParse(entity.customer.phone1, out PhoneNumber p1) ? p1 : new(PhoneNumber.Default),
            Phone2: PhoneNumber.TryParse(entity.customer.phone2, out PhoneNumber p2) ? p2 : new(PhoneNumber.Default),
            Date: ceDate,
            DateCancelled: ceDateCancelled
        );

        // Convert Dates
        DateTimeOffset subDate = _dt.Convert(entity.dateAdded, ETimeZone.Pacific);
        DateTimeOffset cancelDate = entity.dateCancelled != DateTime.MinValue
            ? _dt.Convert(entity.dateCancelled, ETimeZone.Pacific)
            : DateTime.MinValue;

        Sandwich result = new(
            SandId: entity.subscriptionID,
            CustardId: entity.customerID,
            Custard: ce,
            Date: subDate,
            DateCancelled: cancelDate,
            Active: entity.active == 1,
            Complete: entity.initialStatus == 1,
            Type: entity.serviceType ?? "Not Provided",
            Value: entity.contractValue,
            Seller: entity.soldBy ?? 0,
            Seller2: entity.soldBy2 ?? 0,
            Seller3: entity.soldBy3 ?? 0
        );
        return result;
    }
}
