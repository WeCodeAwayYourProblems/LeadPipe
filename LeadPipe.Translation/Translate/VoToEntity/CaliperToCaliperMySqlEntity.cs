using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.VoToEntity;

internal sealed class CaliperToCaliperMySqlEntity : IVoToEntity<Caliper, CaliperMySqlEntity>
{
    public CaliperMySqlEntity Translate(Caliper s)
    {
        int duration = (int)s.Duration.TotalSeconds;
        string billable = s.Billable ? "billable" : "non billable";
        string number = s.Number.Number.ToString();
        string source = s.Source;
        string location = s.Location;
        string note = s.Note;
        DateTime date = s.Date.UtcDateTime;
        return new CaliperMySqlEntity
        {
            duration = duration,
            sale_billable = billable,
            contact_number_clean = number,
            source = source,
            location = location,
            note = note,
            called_at_utc = date
        };
    }
}