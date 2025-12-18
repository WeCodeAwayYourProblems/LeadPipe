using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Settings;

namespace LeadPipe.Infrastructure.Service.Report;

[SourceKey(Source.Yeller)]
public sealed class YellerClientReporter(
    IHttpClientFactory factory,
    IYellerSettings settings
    ) : IReport<ReportYeller>
{
    private readonly IHttpClientFactory _factory = factory;
    private readonly IYellerSettings _settings = settings;
    private HttpClient Client => _factory.CreateClient(_settings.YellerName!);
    public Task<Result> ReportData(List<ReportYeller> d)
    {
        throw new NotImplementedException();
    }
}
