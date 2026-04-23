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
            UnixDate = c.Date.ToUnixTimeSeconds(),
            Payload = c.PayLoad,
            MetaData = c.MetaData,
            Source = c.Source
        };
        return result;
    }
}
