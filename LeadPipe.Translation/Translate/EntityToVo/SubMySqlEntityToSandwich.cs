using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Translation.Primitives;

namespace LeadPipe.Translation.Translate.EntityToVo;

internal sealed class SubMySqlEntityToSandwich(IDateTimeTranslate dt) : IEntityToVo<SubMySqlEntity, Sandwich>
{
    private readonly IDateTimeTranslate _dt = dt;
    public Sandwich Translate(SubMySqlEntity entity)
    {
        CustomerMySqlEntity cust = entity.customer is CustomerMySqlEntity c
            ? c
            : new CustomerMySqlEntity()
            {
                customerID = entity.customerID,
                status = 0,
                phone1 = $"{PhoneNumber.Default}",
                phone2 = $"{PhoneNumber.Default}",
                dateAdded = entity.dateAdded,
                dateCancelled = entity.dateCancelled
            };

        long subscriptionId = entity.subscriptionID;
        long customerId = entity.customerID;

        DateTimeOffset subDate = _dt.Convert(entity.dateAdded, ETimeZone.Pacific);
        DateTimeOffset date = _dt.Convert(cust.dateAdded, ETimeZone.Pacific);

        PhoneNumber number = PhoneNumber.TryParse(cust.phone1, out PhoneNumber n1) ? n1 : new(PhoneNumber.Default);
        PhoneNumber number2 = PhoneNumber.TryParse(cust.phone2, out PhoneNumber n2) ? n2 : new(PhoneNumber.Default);

        DateTimeOffset subCancelDate = DateTime.TryParse(entity.dateCancelled.ToString(), out DateTime scxl)
            ? _dt.Convert(scxl, ETimeZone.Pacific)
            : DateTime.MinValue;
        DateTimeOffset cancelDate = DateTime.TryParse(cust.dateCancelled.ToString(), out DateTime dcxl)
            ? dcxl
            : DateTime.MinValue;

        bool active = cust.status == 1;
        bool subActive = entity.active == 1;
        bool complete = entity.initialStatus == 1;
        decimal value = entity.contractValue;

        const string np = "Not Provided";
        string type = entity.serviceType is string t ? t : np;
        int seller = entity.soldBy is int soldby ? soldby : 0;
        int seller2 = entity.soldBy2 is int soldBy2 ? soldBy2 : 0;
        int seller3 = entity.soldBy3 is int soldBy3 ? soldBy3 : 0;

        Sandwich result = new(
            SubscriptionId: subscriptionId,
            CustomerId: customerId,
            Date: date,
            SubDate: subDate,
            Number: number,
            Number2: number2,
            CancelDate: cancelDate,
            SubCancelDate: subCancelDate,
            Active: active,
            SubActive: subActive,
            Complete: complete,
            Type: type,
            Value: value,
            Seller: seller,
            Seller2: seller2,
            Seller3: seller3
            );
        return result;
    }
}