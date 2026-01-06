using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Attributes;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Settings;

namespace LeadPipe.Infrastructure.Service.Report;

[SourceKey(Source.Pan)]
public sealed class PanReporter(
    ICsvRwService csv,
    IInfrastructureSettings settings
    ) : CsvReporter<ReportPlumbing>(csv, new FileInfo(settings.LabReportLoc!))
{ }
