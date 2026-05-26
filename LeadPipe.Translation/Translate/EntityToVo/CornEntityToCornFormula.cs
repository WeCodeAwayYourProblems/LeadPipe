using LeadPipe.Core;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.EntityToVo;

internal sealed class CornEntityToCornFormula : IEntityToVo<CornEntity, CornFormula>
{
    public CornFormula Translate(CornEntity entity)
    {
        PhoneNumber phoneNumber = new(entity.PhoneNumber);
        DateTimeOffset date = DateTimeOffsetExt.FromUnixTime(entity.UnixDate);
        CornFormula result = new
            (
                Id: entity.Id,
                PhoneNumber: phoneNumber,
                Date: date,
                PayLoad: entity.Payload,
                MetaData: entity.MetaData,
                Source: entity.Source,
                UtmSource: entity.UtmSource,
                UtmMedium: entity.UtmMedium,
                UtmCampaign: entity.UtmCampaign,
                UtmContent: entity.UtmContent,
                UtmTerm: entity.UtmTerm
            );
        return result;
    }
}