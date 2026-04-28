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
        PhoneNumber number = PhoneNumber.TryParse(data.PhoneNumber, out var ph) ? ph : PhoneNumber.DefaultPhoneNumber;
        DateTime d = DateTime.TryParse(data.Date?.Replace("at ", null), out DateTime r)
            ? r
            : DateTime.MaxValue;
        string zoneStr = data.CompletionDate is not null
            ? data.CompletionDate
            : " est";
        ETimeZone zone = zoneStr.Split(" ")[^1].ToLowerInvariant() switch
        {
            "est" or "edt" => ETimeZone.Eastern,
            "cst" or "cdt" => ETimeZone.Central,
            "mst" or "mdt" => ETimeZone.Mountain,
            "pst" or "pdt" => ETimeZone.Pacific,
            _ => ETimeZone.Eastern
        };
        DateTimeOffset date = _dt.Convert(d, zone);
        string contents = data.Contents is null
            ? string.Empty
            : data.Contents;

        string meta1 = data.Lead is string l ? l : "Unknown";
        string meta2 = data.Branch is string b ? b : "Unknown";
        string metadata = $"Is a lead?: {meta1} | Branch: {meta2}";

        Plumbing result = new
            (
                Id: 0,
                PhoneNumber: number,
                Date: date,
                Contents: contents,
                Branch: data.Branch,
                MetaData: metadata,
                Source: Source.Leased,
                [number]
            );
        return result;
    }
}
