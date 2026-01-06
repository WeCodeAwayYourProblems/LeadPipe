using LeadPipe.Infrastructure.Attributes;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Settings;

namespace LeadPipe.Infrastructure.Service.Report;

[ScheduleKey(Schedule.TwoDays)]
public sealed class YellerReporter(
    ICsvRwService csv,
    IInfrastructureSettings settings
    ) : CsvReporter<ReportPlumbing>(csv, new FileInfo(settings.LabReportLoc!))
{ }