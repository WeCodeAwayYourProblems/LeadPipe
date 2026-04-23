using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.DtoToVo;

public sealed class LibacionDtoToPlumbing : IDtoToVo<LibacionDto, Plumbing>
{
    public Plumbing Translate(LibacionDto data)
    {
        PhoneNumber number = PhoneNumber.TryParse(data.PhoneNumber, out PhoneNumber r)
            ? r
            : PhoneNumber.DefaultPhoneNumber;
        DateTimeKind kind = DateTimeKind.Utc;
        DateTime d = DateTime.TryParse(data.Date, out DateTime dt)
            ? DateTime.SpecifyKind(dt, kind)
            : DateTime.SpecifyKind(DateTime.MaxValue, kind);
        DateTimeOffset date = new(d, TimeSpan.Zero);
        string contents = data.Message is null
            ? string.Empty
            : data.Message;

        var isComm = data.Commercial is string c ? c : "Unknown";
        string metadata = $"Is Commercial: {isComm}";

        return new Plumbing(
            Id: 0,
            PhoneNumber: number,
            Date: date,
            Contents: contents,
            Branch: null,
            MetaData: metadata,
            Source.Libacion,
            [number]
            );
    }
}