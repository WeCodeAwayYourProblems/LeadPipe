using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;

namespace LeadPipe.Infrastructure.Translate;

public interface IDtoToVo
{
    public Plumbing Translate(LeafDto v);
}
