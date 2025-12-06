using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.DtoToVo;

internal class LabDtoToPlumbing : IDtoToVo<LabDto, Plumbing>
{
    public Plumbing Translate(LabDto dto)
    {
        throw new NotImplementedException();
    }
}
