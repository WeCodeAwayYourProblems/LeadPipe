using LeadPipe.Core;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.VoToEntity;

internal class SandwichToSandEntity : IVoToEntity<Sandwich, SandEntity>
{
    public SandEntity Translate(Sandwich s)
    {
        long? dateCancelled = s.DateCancelled?.ToUnixTime() < 0 || s.DateCancelled?.ToUnixTime() == DateTimeOffset.MaxValue.ToUnixTime() 
            ? null 
            : s.DateCancelled?.ToUnixTime();
        return new SandEntity()
        {
            Id = s.SandId,
            CustardId = s.CustardId,
            Date = s.Date.UtcDateTime,
            UnixDate = s.Date.ToUnixTime(),
            UnixCancelDate = dateCancelled,
            Active = s.Active,
            Complete = s.Complete,
            Type = s.Type,
            Value = s.Value,
            Seller = s.Seller,
            Seller2 = s.Seller2,
            Seller3 = s.Seller3,
            Offerman = s.Offerman,
        };
    }
}
