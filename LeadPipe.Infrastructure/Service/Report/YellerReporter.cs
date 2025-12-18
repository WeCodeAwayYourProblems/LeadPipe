using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Settings;

namespace LeadPipe.Infrastructure.Service.Report;

[SourceKey(Source.Yeller)]
public sealed class YellerReporter(
    ICsvRwService csv,
    IInfrastructureSettings settings
    ) : CsvReporter<ReportPlumbing>(csv, new FileInfo(settings.LabReportLoc!))
{ }