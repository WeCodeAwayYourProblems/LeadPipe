using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.EntityToVo;

internal class CallEntityToCall : IEntityToVo<CallEntity, Call>
{
    public Call Translate(CallEntity c)
    {
        var utc = DateTime.SpecifyKind(c.CallDate, DateTimeKind.Utc);
        DateTimeOffset date = new(utc, TimeSpan.Zero);
        PhoneNumber number = new(c.PhoneNumber);
        TimeSpan duration = TimeSpan.FromSeconds(c.Duration);
        string note = c.Note;
        string source = c.Source;
        string location = c.Location;
        bool billable = c.Billable;

        Call result = 
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
