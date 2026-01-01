using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Application.Service;

public interface IUpdateSourceFactory
{
    public IUpdateService<Plumbing> GetService(Source source);
}
