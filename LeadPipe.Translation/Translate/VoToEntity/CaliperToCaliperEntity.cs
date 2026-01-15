using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.VoToEntity;

internal class CaliperToCaliperEntity : IVoToEntity<Caliper, CaliperEntity>
{
    public CaliperEntity Translate(Caliper c)
    {
        var result = new CaliperEntity()
        {
            Id = c.Id,
            PhoneNumber = c.Number.Number,
            Date = c.Date.UtcDateTime,
            UnixDate = c.Date.ToUnixTimeSeconds(),
            Note = c.Note,
            Source = c.Source,
            Location = c.Location,
            Duration = (int)c.Duration.TotalSeconds,
            Billable = c.Billable
        };
        return result;
    }
}
