using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Application.Manager;

public interface IReportManager
{
    Task<Result> Manage(Source source);
    Task<Result> Manage();
}
public sealed class ReportManager(
    IReportSourceFactory report
    ) : IReportManager
{
    private readonly IReportSourceFactory _report = report;
    public async Task<Result> Manage(Source source)
    {
        IReportService<Plumbing> reporter = _report.GetService(source);
        Result<List<Plumbing>> reportData = await reporter.GetDataAsync(false);
        Result reported = reportData.IsSuccess
            ? await reporter.ReportAsync(reportData.Value)
            : reportData;
        return reported;
    }

    public async Task<Result> Manage()
    {
        Source[] sources = [.. Enum.GetValues<Source>().Except([Source.Test, Source.Test2])];
        List<Result> results = new(sources.Length);
        foreach (Source source in sources)
        {
            Result reported = await Manage(source);
            results.Add(reported);
        }
        return Result.Combine(" | ", [.. results]);
    }
}
