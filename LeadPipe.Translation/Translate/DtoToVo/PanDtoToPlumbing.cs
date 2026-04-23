using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Translation.Primitives;

namespace LeadPipe.Translation.Translate.DtoToVo;

internal class PanDtoToPlumbing(IDateTimeTranslate dt) : IDtoToVo<PanDto, Plumbing>
{
    private readonly IDateTimeTranslate _dt = dt;
    public Plumbing Translate(PanDto data)
    {
        PhoneNumber number = PhoneNumber.TryParse(data.Number, out var ph) ? ph : PhoneNumber.DefaultPhoneNumber;
        DateTime d = DateTime.TryParse(data.Date, out DateTime r)
            ? r
            : DateTime.MaxValue;
        DateTimeOffset date = new(DateTime.SpecifyKind(d, DateTimeKind.Utc), TimeSpan.Zero);
        string contents = data.Content is null
            ? string.Empty
            : data.Content;
        string meta = data.Metadata is string m ? m : string.Empty;
        return new(
            Id: 0,
            PhoneNumber: number,
            Date: date,
            Contents: contents,
            Branch: null,
            MetaData: meta,
            Source.Pan,
            [number]
            );
    }
}