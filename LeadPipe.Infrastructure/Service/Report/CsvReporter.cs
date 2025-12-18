using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Service;

namespace LeadPipe.Infrastructure.Service.Report;

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