using LeadPipe.Infrastructure.Interfaces.Core;

namespace LeadPipe.Infrastructure.Interfaces.Service;

internal interface IFirstTouchService
{
    TLink? GetFirstTouch<TLink>(IEnumerable<TLink> links)
        where TLink : IEntity, IHasUnixMatchDate;
}
