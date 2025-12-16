using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Service;
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

[SourceKey(Source.Calli)]
public sealed class CalliReporter(
    ICsvRwService csv,
    IInfrastructureSettings settings
    ) : CsvReporter<ReportFilePlumbing>(csv, new FileInfo(settings.CalliReportLoc!))
{ }

public abstract class CsvReporter<TReport>(
    ICsvRwService csv,
    FileInfo file
    ) : IReport<TReport>
{
    private readonly ICsvRwService _csv = csv;
    private readonly FileInfo _file = file;
    public virtual Task<Result> ReportData(List<TReport> d)
    {
        Result result = _csv.Write(d, _file);
        return Task.FromResult(result);
    }
}