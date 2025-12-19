using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.VoToEntity;

internal class CallToCallEntity : IVoToEntity<Call, CallEntity>
{
    public CallEntity Translate(Call c)
    {
        var result = new CallEntity()
        {
            PhoneNumber = c.Number.Number,
            CallDate = c.Date.UtcDateTime,
            UnixCallDate = c.Date.ToUnixTimeSeconds(),
            Note = c.Note,
            Source = c.Source,
            Location = c.Location,
            Duration = (int)c.Duration.TotalSeconds,
            Billable = c.Billable
        };
        return result;
    }
}
