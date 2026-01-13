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
        if (entity.CustardEntity is null)
            throw new ArgumentException($"Navigation property {nameof(entity.CustardEntity)} cannot be null.");

        CustardEntity ce = entity.CustardEntity;
        Custard custard = new(
                Id: ce.Id,
                Status: ce.Active,
                Phone1: new PhoneNumber(ce.PhoneNumber),
                Phone2: new PhoneNumber(ce.PhoneNumber2),
                Date: ce.Date,
                DateCancelled: ce.CancelDate
            );
        Sandwich result = new
        (
            SandId: entity.Id,
            CustardId: entity.CustardId,
            Custard: custard,
            Date: _dt.Convert(entity.Date, ETimeZone.Pacific),
            DateCancelled: _dt.Convert(entity.CancelDate, ETimeZone.Pacific),
            Active: entity.Active,
            Complete: entity.Complete,
            Type: entity.Type,
            Value: entity.Value,
            Seller: entity.Seller,
            Seller2: entity.Seller2,
            Seller3: entity.Seller3
        );
        return result;
    }
}
