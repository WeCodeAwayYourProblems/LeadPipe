using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.DtoToVo;

internal class LabDtoToPlumbing : IDtoToVo<LabDto, Plumbing>
{
    const string _unk = "Unknown";
    public Plumbing Translate(LabDto dto)
    {
        var buyers = dto.entities?
            .Where(e => e?.buyer is not null)
            .Select(e => e!.buyer!)
            .OfType<Buyer>()!;
        var input = buyers
            .Where(v => v?.telephone is not null)
            .Select(v => v!.telephone!)
            .OfType<string>()
            .ToArray();
        var numbers = PhoneNumber.TryParseMany(input, out List<PhoneNumber>? p) && p.Count > 0
            ? p.ToArray()
            : [PhoneNumber.DefaultPhoneNumber];
        var canonicalPhoneNumber = numbers[^1];

        DateTimeOffset date = DateTimeOffset.TryParse(dto.created_at?.date_utc, out DateTimeOffset r) ? r : DateTimeOffset.MinValue;

        string contents = dto.display?.text is null ? string.Empty : dto.display.text;

        string branch = _unk;
        string location = dto.metadata?.location?.name is null ? _unk : dto.metadata.location.name;
        string metaData = $"Location: {location}";

        Plumbing result = new(
            Id: 0,
            PhoneNumber: canonicalPhoneNumber,
            Date: date,
            Contents: contents,
            Branch: branch,
            MetaData: metaData,
            Source: Source.Lab,
            Numbers: numbers
            );

        return result;
    }
}
