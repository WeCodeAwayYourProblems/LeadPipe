using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.EntityToVo;

internal class CaliperEntityToCaliper : IEntityToVo<CaliperEntity, Caliper>
{
    public Caliper Translate(CaliperEntity c)
    {
        DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(c.UnixDate);
        PhoneNumber number = new(c.PhoneNumber);
        TimeSpan duration = TimeSpan.FromSeconds(c.Duration);
        string note = c.Note;
        string source = c.Source;
        string location = c.Location;
        bool billable = c.Billable;

        Caliper result = 
            new(
                Id: c.Id,
                Date: date, 
                Number: number, 
                Duration: duration, 
                Note: note, 
                Source: source, 
                Location: location, 
                Billable: billable
            );
        return result;
    }
}
