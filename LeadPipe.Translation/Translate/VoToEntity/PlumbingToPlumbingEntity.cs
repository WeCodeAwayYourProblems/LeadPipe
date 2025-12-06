using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.VoToEntity;

internal class PlumbingToPlumbingEntity : IVoToEntity<Plumbing, PlumbingEntity>
{
    public PlumbingEntity Translate(Plumbing plumbing)
    {
        var result = new PlumbingEntity()
        {
            PhoneNumber = plumbing.PhoneNumber.Number,
            Date = plumbing.Date.UtcDateTime,
            UnixDate = plumbing.Date.ToUnixTimeSeconds(),
            Contents = plumbing.Contents,
            Source = plumbing.Source,
        };
        return result;
    }
}
