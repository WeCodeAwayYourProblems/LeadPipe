using LeadPipe.Core;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Translation.Translate.VoToEntity;

namespace LeadPipe.Translation.Translate.DtoToVo;

internal sealed class CatManDtoToCornFormula : IDtoToVo<CatManDto, CornFormula>
{
    public CornFormula Translate(CatManDto data)
    {
        long unix = (long)(data.unix_time is null ? 0 : data.unix_time);
        DateTimeOffset date = DateTimeOffsetExt.FromUnixTime(unix);

        PhoneNumber number = new(data.caller_number_bare);

        string payLoad = data.ToString();
        string referring = data.location ?? "None";
        string metaData = $"{CornMySqlEntityTranslationHelper.ReferringValue}{referring}";
        string source = data.source ?? "Unknown";

        var result = new CornFormula(
            Id: data.id,
            PhoneNumber: number,
            Date: date,
            PayLoad: payLoad,
            MetaData: metaData,
            Source: source
            );

        return result;
    }

}