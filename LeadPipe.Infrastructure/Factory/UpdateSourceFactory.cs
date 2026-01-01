using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Infrastructure.Factory;

class UpdateSourceFactory(IServiceProvider provider) : IUpdateSourceFactory
{
    private readonly IServiceProvider _provider = provider;
    public IUpdateService<Plumbing> GetService(Source source)
    {
        return _provider.GetRequiredKeyedService<IUpdateService<Plumbing>>(source);
    }
}
