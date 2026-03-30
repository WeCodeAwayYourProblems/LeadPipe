using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Translation.Primitives;

namespace LeadPipe.Translation.Translate.DtoToVo;

internal class LatherDtoToPlumbing(IDateTimeTranslate translate) : IDtoToVo<LatherDto, Plumbing>
{
    private readonly IDateTimeTranslate _translate = translate;
    public Plumbing Translate(LatherDto data)
    {
        long id = long.TryParse(data.LeadId, out long i) ? i : 0;
        PhoneNumber phoneNumber = PhoneNumber.TryParse(data.Phone, out PhoneNumber p) ? p : new(PhoneNumber.Default);

        DateTime dt = DateTime.TryParse(string.Join(" ", [data.Date, data.Time]), out DateTime d) ? d : DateTime.MinValue;
        ETimeZone zone = data.TimeZone?.ToLowerInvariant() switch
        {
            "est" or "edt" => ETimeZone.Eastern,
            "pst" or "pdt" => ETimeZone.Pacific,
            "cst" or "cdt" => ETimeZone.Central,
            "utc" => ETimeZone.Utc,
            _ => ETimeZone.Mountain, // Default to mountain

        };
        DateTimeOffset date = _translate.Convert(dt, zone);
        string metaData = $"Lead Id: {id}";

        Plumbing result = new(
            Id: id,
            PhoneNumber: phoneNumber,
            Date: date,
            Contents: null,
            Branch: null,
            MetaData: metaData,
            Source: Source.Lather,
            null
            );

        return result;
    }
}