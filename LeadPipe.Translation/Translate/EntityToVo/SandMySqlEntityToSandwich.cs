using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Translation.Primitives;

namespace LeadPipe.Translation.Translate.EntityToVo;

internal sealed class SandMySqlEntityToSandwich(IDateTimeTranslate dt) : IEntityToVo<SandMySqlEntity, Sandwich>
{
    private readonly IDateTimeTranslate _dt = dt;

    public Sandwich Translate(SandMySqlEntity entity)
    {
        if (entity.customer is null && entity.offerman is null)
            throw new ArgumentException($"{nameof(entity.customer)} cannot be null. {nameof(entity.offerman)} cannot be null.");
        else if (entity.offerman is null)
            throw new ArgumentException($"{nameof(entity.offerman)} cannot be null.");
        else if (entity.customer is null)
            throw new ArgumentException($"{nameof(entity.customer)} cannot be null.");

        // Convert navigation property
        DateTime cAdded = entity.customer.dateAdded is DateTime cAdd
            ? cAdd : DateTime.MinValue;
        DateTime cCxl = entity.customer.dateCancelled is DateTime cxl
            ? cxl : DateTime.MinValue;
        DateTimeOffset ceDate = _dt.Convert(cAdded, ETimeZone.Pacific);
        DateTimeOffset ceDateCancelled = _dt.Convert(cCxl, ETimeZone.Pacific);
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
        DateTimeOffset subDate = entity.dateAdded is DateTime added
            ? _dt.Convert(added, ETimeZone.Pacific)
            : DateTime.MinValue;
        DateTimeOffset? cancelDate = entity.dateCancelled is not null && entity.dateCancelled != DateTime.MinValue
            ? _dt.Convert((DateTime)entity.dateCancelled, ETimeZone.Pacific)
            : null;

        // Get offerman
        string offerman = entity.offerman.branchName is null ? "Unknown" : entity.offerman.branchName;

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
            Seller3: entity.soldBy3 ?? 0,
            Offerman: offerman
        );
        return result;
    }
}
