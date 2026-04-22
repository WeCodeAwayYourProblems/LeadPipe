using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Translation.Primitives;

namespace LeadPipe.Translation.Translate.DtoToVo;

internal class CalliDtoToPlumbing(IDateTimeTranslate dt) : IDtoToVo<CalliDto, Plumbing>
{
    private readonly IDateTimeTranslate _dt = dt;
    public Plumbing Translate(CalliDto v)
    {
        PhoneNumber phone = PhoneNumber.TryParse(v.Phone, out var p) ? p : new(PhoneNumber.Default);
        DateTime datetime = DateTime.TryParse(v.Date + " " + v.Time, out DateTime dt)
            ? dt
            : DateTime.MaxValue;
        string rawZone = v.TimeZone is null ? "mst" : v.TimeZone.ToLowerInvariant();
        ETimeZone zone = rawZone switch
        {
            "pst" or "pdt" => ETimeZone.Pacific,

            "mst" or "mdt" => ETimeZone.Mountain,

            "cst" or "cdt" => ETimeZone.Central,
            "est" or "edt" => ETimeZone.Eastern,
            "utc" or _ => ETimeZone.Utc,
        };
        DateTimeOffset date = _dt.Convert(DateTime.SpecifyKind(datetime, DateTimeKind.Unspecified), zone);
        var meta = v.Metadata is string m ? m : string.Empty;

        return new Plumbing
        (
            Id: 0,
            PhoneNumber: phone,
            Date: date,
            Contents: v.PestProblem,
            Branch: null,
            MetaData: meta,
            Source: Source.Calli,
            [phone]
        );
    }
}
