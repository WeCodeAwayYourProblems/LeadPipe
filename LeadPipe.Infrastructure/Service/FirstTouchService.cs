using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Service;

namespace LeadPipe.Infrastructure.Service;

internal class FirstTouchService : IFirstTouchService
{
    public TLink? GetFirstTouch<TLink>(IEnumerable<TLink> links) where TLink : IEntity, IHasUnixMatchDate
    {
        return links
            .OrderBy(l => l.UnixMatchDate)
            .FirstOrDefault();
    }
}
