using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Settings;

namespace LeadPipe.Infrastructure.Service.Report;

[SourceKey(Source.Yeller)]
public sealed class YellerClientReporter : IReport<ReportYeller>
{
    private readonly IHttpClientFactory _factory;
    private readonly IYellerSettings _settings;
    private readonly HttpClient _client;

    public YellerClientReporter(
        IHttpClientFactory factory,
        IYellerSettings settings
    )
    {
        _factory = factory;
        _settings = settings;
        _client = _factory.CreateClient(_settings.YellerName!);
    }

    public Task<Result> ReportData(List<ReportYeller> d)
    {
        throw new NotImplementedException();
    }
}
