using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;

namespace LeadPipe.Infrastructure.Translate;

public interface IDtoToVo
{
    Plumbing Translate(LeafDto v);
    Plumbing Translate(CalliCsvDto v);
}
