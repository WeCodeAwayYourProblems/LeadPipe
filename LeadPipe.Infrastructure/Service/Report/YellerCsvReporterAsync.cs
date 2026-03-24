using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Settings;

namespace LeadPipe.Infrastructure.Service.Report;

public sealed class YellerCsvReporterAsync(
    IYellerSettings settings,
    ICsvRwService csv
    ) : IReport<ReportYeller>
{
    private readonly FileInfo _loc = new(settings.YellerCsvReporterLoc!);
    private readonly ICsvRwService _csv = csv;

    public async Task<Result> ReportData(List<ReportYeller> d)
    {
        Result result = await _csv.WriteAsync(d, _loc);
        return result;
    }
}