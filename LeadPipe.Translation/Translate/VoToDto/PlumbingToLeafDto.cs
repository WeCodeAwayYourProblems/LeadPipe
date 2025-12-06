using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.VoToDto;

internal class PlumbingToLeafDto : IVoToDto<Plumbing, LeafDto>
{
    public LeafDto Translate(Plumbing p)
    {
        string cellphone = $"{p.PhoneNumber.Number}";
        DateTime creation = p.Date.UtcDateTime;
        string? message = p.Contents;
        LeafDto result = new()
        {
            creation = creation,
            prospect = new() { cellphone = cellphone },
            messages = [new Message() { message = message }]
        };
        return result;
    }
}
