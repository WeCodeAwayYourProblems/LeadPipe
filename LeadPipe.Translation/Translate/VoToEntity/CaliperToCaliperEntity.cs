using LeadPipe.Core;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.VoToEntity;

internal class CaliperToCaliperEntity : IVoToEntity<Caliper, CaliperEntity>
{
    public CaliperEntity Translate(Caliper c)
    {
        var result = new CaliperEntity()
        {
            Id = c.Id,
            PhoneNumber = c.Number,
            Date = c.Date.UtcDateTime,
            UnixDate = c.Date.ToUnixTime(),
            Note = c.Note,
            Source = c.Source,
            Label = c.Label,
            Location = c.Location,
            Duration = (int)c.Duration.TotalSeconds,
            Billable = c.Billable
        };
        return result;
    }
}
