using LeadPipe.Core;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.VoToEntity;

internal class PlumbingToPlumbingEntity : IVoToEntity<Plumbing, PlumbingEntity>
{
    public PlumbingEntity Translate(Plumbing plumbing)
    {
        ICollection<PlumbingPhoneNumber> numbers = plumbing.Numbers is not null
            ? [
                .. plumbing.Numbers.Select(p => new PlumbingPhoneNumber() 
                    {
                        PhoneNumber = new(p),
                        PlumbingId = plumbing.Id
                    })
              ]
            : [];

        var result = new PlumbingEntity()
        {
            Id = plumbing.Id,
            PhoneNumber = plumbing.PhoneNumber,
            Date = plumbing.Date.UtcDateTime,
            UnixDate = plumbing.Date.ToUnixTime(),
            Contents = plumbing.Contents,
            MetaData = plumbing.MetaData,
            Source = plumbing.Source,
            Branch = plumbing.Branch,
            PhoneNumbers = numbers
        };
        return result;
    }
}
