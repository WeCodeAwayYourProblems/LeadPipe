using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.EntityToVo;

internal class CallMySqlEntityToCall : IEntityToVo<CallMySqlEntity, Call>
{
    public Call Translate(CallMySqlEntity entity)
    {
        DateTime d = DateTime.SpecifyKind(entity.called_at_utc, DateTimeKind.Utc);
        DateTimeOffset date = new(d, TimeSpan.Zero);

        PhoneNumber number = PhoneNumber.TryParse(entity.contact_number_clean, out PhoneNumber p) ? p : new(PhoneNumber.Default);

        double dur = entity.duration is int dura ? dura : 0;
        TimeSpan duration = TimeSpan.FromSeconds(dur);

        string? transcription = entity.transcriptions.Count == 0
            ? null
            : string.Join(" | ", entity.transcriptions.Where(t => t.transcription is not null).Select(t => t.transcription));
        string? summary = entity.summaries.Count == 0
            ? null
            : string.Join(" | ", entity.summaries.Where(s => s.summary is not null).Select(s => s.summary));
        string notes = string.Join(" | ", summary, transcription);

        string source = entity.source is string s ? s : string.Empty;
        string location = entity.location is string l ? l : string.Empty;
        bool billable = entity.sale_billable is not null && entity.sale_billable == "billable";

        Call result =
            new(
                Id: entity.call_id,
                Date: date,
                Number: number,
                Duration: duration,
                Note: notes,
                Source: source,
                Location: location,
                Billable: billable
            );
        return result;
    }
}