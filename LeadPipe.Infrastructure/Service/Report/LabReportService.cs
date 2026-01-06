using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Attributes;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Infrastructure.Service.Report;

[SourceKey(Source.Lab)]
internal sealed class LabReportService(
    [FromKeyedServices(Source.Lab)] ILoadData<Plumbing> load,
    ITransform<Plumbing, ReportPlumbing> transform,
    [FromKeyedServices(Source.Lab)] IReport<ReportPlumbing> report
    ) : ReportService<Plumbing, ReportPlumbing>(load, transform, report)
{ }
