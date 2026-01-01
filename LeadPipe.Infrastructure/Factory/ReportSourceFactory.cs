using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Infrastructure.Factory;

internal class ReportSourceFactory(IServiceProvider provider) : IReportSourceFactory
{
    private readonly IServiceProvider _provider = provider;
    public IReportService<Plumbing> GetService(Source source)
    {
        return _provider.GetRequiredKeyedService<IReportService<Plumbing>>(source);
    }
}
