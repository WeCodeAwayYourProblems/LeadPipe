using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Infrastructure.Settings;
using LeadPipe.Translation.Translate.VoToEntity;

namespace LeadPipe.Translation.Translate.EntityToVo;

internal sealed class CornMySqlEntityToCornFormula(IInfrastructureSettings settings) : IEntityToVo<CornMySqlEntity, CornFormula>
{
    private readonly string[] _sources = settings.CornSources ?? [];
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
        string form = entity.form ?? "None";
        string referring = entity.referringURL ?? "None";
        string metadata = $"{CornMySqlEntityTranslationHelper.FormValue}{form}{CornMySqlEntityTranslationHelper.Delimiter}{CornMySqlEntityTranslationHelper.ReferringValue}{referring}";

        // Find sources
        List<string> source = [];
        foreach (var s in _sources)
            if (form.Contains(s) || referring.Contains(s))
                source.Add(s);

        CornFormula result = new(
            Id: entity.id,
            PhoneNumber: phoneNumber,
            Date: date,
            PayLoad: payload,
            MetaData: metadata,
            Source: string.Join(" | ", source),
            UtmSource: entity.source,
            UtmMedium: entity.medium,
            UtmCampaign: entity.campaign,
            UtmContent: entity.utmContent,
            UtmTerm: entity.utmTerm
        );

        return result;
    }
}
