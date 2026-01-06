using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Attributes;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Settings;

namespace LeadPipe.Infrastructure.Service.Report;

[ScheduleKey(Schedule.Daily)]
public sealed class YellerJsonReporter(
    IYellerSettings settings,
    IJsonRwService json
    ) : IReport<ReportYeller>
{
    private readonly IYellerSettings _settings = settings;
    private readonly IJsonRwService _json = json;

    public async Task<Result> ReportData(List<ReportYeller> d)
    {
        FileInfo loc = new(_settings.YellerClientReporterLoc!);
        return await _json.WriteToFileAsync(loc, d);
    }
}
