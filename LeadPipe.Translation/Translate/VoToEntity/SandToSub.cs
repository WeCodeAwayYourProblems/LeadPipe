using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Translate;

namespace LeadPipe.Translation.Translate.VoToEntity;

internal class SandToSub : IVoToEntity<Sandwich, SubsEntity>
{
    public SubsEntity Translate(Sandwich s)
    {
        var result = new SubsEntity()
        {
            Id = s.SubscriptionId,
            CustomerId = s.CustomerId,
            Date = s.Date.UtcDateTime,
            UnixDate = s.Date.ToUnixTimeSeconds(),
            SubDate = s.SubDate.UtcDateTime,
            UnixSubDate = s.SubDate.ToUnixTimeSeconds(),
            Number = s.Number.Number,
            Number2 = s.Number2.Number,
            CancelDate = s.CancelDate.UtcDateTime,
            UnixCancelDate = s.CancelDate.ToUnixTimeSeconds(),
            SubCancelDate = s.SubCancelDate.UtcDateTime,
            UnixSubCancelDate = s.SubCancelDate.ToUnixTimeSeconds(),
            Active = s.Active,
            SubActive = s.SubActive,
            Complete = s.Complete,
            Value = s.Value,
            Seller = s.Seller,
            Seller2 = s.Seller2,
            Seller3 = s.Seller3
        };
        return result;
    }
}
