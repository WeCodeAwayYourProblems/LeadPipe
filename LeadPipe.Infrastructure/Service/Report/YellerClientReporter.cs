using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Settings;

namespace LeadPipe.Infrastructure.Service.Report;

[SourceKey(Source.Yeller)]
public sealed class YellerClientReporter : IReport<ReportYeller>
{
    private readonly IYellerSettings _settings;
    private readonly HttpClient _client;

    public YellerClientReporter(
        IHttpClientFactory factory,
        IYellerSettings settings
    )
    {
        _settings = settings;
        _client = factory.CreateClient(_settings.YellerName!);
    }

    public async Task<Result> ReportData(List<ReportYeller> d)
    {
        throw new NotImplementedException();
    }
}
