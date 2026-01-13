using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.EntityToVo;

internal class CornMySqlEntityToCornFormula : IEntityToVo<CornMySqlEntity, CornFormula>
{
    public CornFormula Translate(CornMySqlEntity entity)
    {
        // Date
        DateTime timestamp = DateTime.SpecifyKind(entity.timestamp, DateTimeKind.Utc);
        DateTimeOffset date = new(timestamp, TimeSpan.Zero);

        // Phone
        PhoneNumber phoneNumber = PhoneNumber.TryParse(entity.phoneNumber, out var pn)
            ? pn
            : new PhoneNumber(PhoneNumber.Default);

        // Data
        string payload = entity.comments ?? string.Empty;
        string metadata = $"Form: {entity.form ?? "None"} | Referring: {entity.referringURL ?? "None"}";
        string source = entity.source ?? string.Empty;

        CornFormula result = new(
            Id: entity.id,
            PhoneNumber: phoneNumber,
            Date: date,
            PayLoad: payload,
            MetaData: metadata,
            Source: source
        );

        return result;
    }
}
