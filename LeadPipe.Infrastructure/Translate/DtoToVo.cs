using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Translation.Primitives;

namespace LeadPipe.Infrastructure.Translate;

internal class DtoToVo(IDateTimeTranslate dt) : IDtoToVo
{
    private readonly IDateTimeTranslate _dt = dt;
    public Plumbing Translate(LeafDto v)
    {
        // PhoneNumber
        string? cellPhone = v.prospect is not null
            ? v.prospect.cellphone
            : PhoneNumber.Default.ToString();
        PhoneNumber number = PhoneNumber.TryParse(cellPhone, out PhoneNumber p)
            ? p
            : new(PhoneNumber.Default);

        // Date
        DateTime d = v.creation;
        DateTimeOffset date = _dt.Convert(d, TimeSpan.FromHours(0));

        // Contents 
        string content = string.Empty;
        if (v.messages is not null)
        {
            var soonest = DateTime.MaxValue;
            Message soonestMsg = new() { creation = soonest, message = null };
            foreach (var m in v.messages)
                if (m.creation < soonest)
                {
                    soonest = m.creation;
                    soonestMsg = m;
                }
            content = soonestMsg.message is null
                ? string.Empty
                : soonestMsg.message;
        }

        Plumbing result = new(PhoneNumber: number, Date: date, Contents: content, Source: Source.Leaf);
        return result;
    }
}
