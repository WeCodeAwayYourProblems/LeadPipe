using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.DtoToVo;

internal class LeafDtoToPlumbing : IDtoToVo<LeafDto, Plumbing>
{
    // TODO: ensure that leaf gets the right information, please, especially metadata
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
        DateTime d = DateTime.SpecifyKind(v.creation, DateTimeKind.Utc);
        DateTimeOffset date = new(d, TimeSpan.Zero);

        // Contents 
        string content = string.Empty;
        List<string> metastr = [];
        if (v.messages is not null)
        {
            var soonest = DateTime.MaxValue;
            Message soonestMsg = new() { creation = soonest, message = null };
            foreach (Message m in v.messages)
            {
                // Find soonest message
                if (m.creation < soonest)
                {
                    soonest = m.creation;
                    soonestMsg = m;
                }

                // Find message source
                if (m.source is not null)
                    metastr.Add(m.source);
            }
            content = soonestMsg.message is null
                ? string.Empty
                : soonestMsg.message;
        }
        string metadata = metastr.Count == 0 ? string.Empty : string.Join(" | ", metastr.Where(m => !string.IsNullOrWhiteSpace(m)));

        Plumbing result = new
        (
            Id: 0,
            PhoneNumber: number,
            Date: date,
            Contents: content,
            Branch: null,
            MetaData: metadata,
            Source: Source.Leaf,
            null
        );
        return result;
    }
}
