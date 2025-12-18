using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Settings;

namespace LeadPipe.Infrastructure.Service.Report;

[SourceKey(Source.Lab)]
public sealed class LabReporter(
    ICsvRwService csv,
    IInfrastructureSettings settings
    ) : CsvReporter<ReportPlumbing>(csv, new FileInfo(settings.LabReportLoc!))
{ }
