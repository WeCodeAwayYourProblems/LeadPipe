using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Translation.Primitives;

namespace LeadPipe.Translation.Translate.EntityToVo;

internal class SandToSandwich(IDateTimeTranslate dtranslate) : IEntityToVo<SandEntity, Sandwich>
{
    private readonly IDateTimeTranslate _dt = dtranslate;
    public Sandwich Translate(SandEntity entity)
    {
        var result = new Sandwich
        (
            SubscriptionId: entity.Id,
            CustomerId: entity.CustomerId,
            Date: _dt.Convert(entity.Date, ETimeZone.Pacific),
            SubDate: _dt.Convert(entity.SubDate, ETimeZone.Pacific),
            Number: new(entity.PhoneNumber),
            Number2: new(entity.PhoneNumber2),
            CancelDate: _dt.Convert(entity.CancelDate, ETimeZone.Pacific),
            SubCancelDate: _dt.Convert(entity.SubCancelDate, ETimeZone.Pacific),
            Active: entity.Active,
            SubActive: entity.SubActive,
            Complete: entity.Complete,
            Type: entity.Type,
            Value: (decimal)entity.Value,
            Seller: entity.Seller,
            Seller2: entity.Seller2,
            Seller3: entity.Seller3
        );
        return result;
    }
}
