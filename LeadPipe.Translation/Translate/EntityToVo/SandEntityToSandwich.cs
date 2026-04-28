using LeadPipe.Core;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.EntityToVo;

internal class SandEntityToSandwich(IEntityToVo<CustardEntity, Custard> eToVo) : IEntityToVo<SandEntity, Sandwich>
{
    private readonly IEntityToVo<CustardEntity, Custard> _eToVo = eToVo;
    public Sandwich Translate(SandEntity entity)
    {
        if (entity.CustardEntity is null)
            throw new ArgumentException($"Navigation property {nameof(entity.CustardEntity)} cannot be null.");

        CustardEntity ce = entity.CustardEntity;
        var custard = _eToVo.Translate(ce);
        DateTimeOffset? dateCancelled = entity.UnixCancelDate is null ? null : DateTimeOffsetExt.FromUnixTime((long)entity.UnixCancelDate);

        Sandwich result = new
        (
            SandId: entity.Id,
            CustardId: entity.CustardId,
            Custard: custard,
            Date: DateTimeOffsetExt.FromUnixTime(entity.UnixDate),
            DateCancelled: dateCancelled,
            Active: entity.Active,
            Complete: entity.Complete,
            Type: entity.Type,
            Value: entity.Value,
            Seller: entity.Seller,
            Seller2: entity.Seller2,
            Seller3: entity.Seller3,
            Offerman: entity.Offerman
        );
        return result;
    }
}
