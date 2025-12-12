using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Translation.Primitives;

namespace LeadPipe.Translation.Translate.DtoToVo;

public class LeasedDtoToPlumbing(IDateTimeTranslate dt) : IDtoToVo<LeasedDto, Plumbing>
{
    private readonly IDateTimeTranslate _dt = dt;
    public Plumbing Translate(LeasedDto data)
    {
        PhoneNumber number = new(data.PhoneNumber);
        DateTime d = DateTime.TryParse(data.Date, out DateTime r)
            ? r
            : DateTime.MaxValue;
        ETimeZone z = (data.CompletionDate is not null
            ? data.CompletionDate
            : " est").Split(" ")[^1].ToLowerInvariant() switch
        {
            "est" or "edt" => ETimeZone.Eastern,
            "cst" or "cdt" => ETimeZone.Central,
            "mst" or "mdt" => ETimeZone.Mountain,
            "pst" or "pdt" => ETimeZone.Pacific,
            _ => ETimeZone.Eastern
        };
        DateTimeOffset date = _dt.Convert(d, z);
        string contents = data.Contents is null
            ? string.Empty
            : data.Contents;

        Plumbing result = new(PhoneNumber: number, Date: date, Contents: contents, Source: Source.Leased);
        return result;
    }
}