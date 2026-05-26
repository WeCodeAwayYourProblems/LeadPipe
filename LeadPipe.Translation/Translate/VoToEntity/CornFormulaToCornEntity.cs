using LeadPipe.Core;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.VoToEntity;

internal sealed class CornFormulaToCornEntity : IVoToEntity<CornFormula, CornEntity>
{
    public CornEntity Translate(CornFormula c)
    {
        var result = new CornEntity()
        {
            Id = c.Id,
            PhoneNumber = c.PhoneNumber,
            Date = c.Date.UtcDateTime,
            UnixDate = c.Date.ToUnixTime(),
            Payload = c.PayLoad,
            MetaData = c.MetaData,
            Source = c.Source,
            UtmSource = c.UtmSource,
            UtmMedium = c.UtmMedium,
            UtmCampaign = c.UtmCampaign,
            UtmContent = c.UtmContent,
            UtmTerm = c.UtmTerm
        };
        return result;
    }
}
