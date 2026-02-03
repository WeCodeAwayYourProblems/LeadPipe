using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.DtoToVo;

internal class LabDtoToPlumbing : IDtoToVo<LabDto, Plumbing>
{
    public Plumbing Translate(LabDto dto)
    {
        PhoneNumber number = dto.entities?.buyer is not null && PhoneNumber.TryParse(dto.entities?.buyer.telephone, out PhoneNumber p)
            ? p
            : new(PhoneNumber.Default);

        DateTime d = DateTime.TryParse(dto.created_at?.date_utc, out DateTime r) ? r : DateTime.MinValue;
        DateTimeOffset date = new(d, TimeSpan.Zero);

        string contents = dto.display?.text is null ? string.Empty : dto.display.text;

        string branch = "Unknown";
        string location = dto.metadata?.location?.name is null ? "Unknown" : dto.metadata.location.name;
        string metaData = $"Location: {location}";

        Plumbing result = new(
            Id: 0, 
            PhoneNumber: number, 
            Date: date, 
            Contents: contents, 
            Branch: branch, 
            MetaData: metaData, 
            Source: Source.Lab);
        
        return result;
    }
}
