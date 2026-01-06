using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Attributes;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Infrastructure.Service.Report;

[SourceKey(Source.Calli)]
internal sealed class CalliReportService(
    [FromKeyedServices(Source.Calli)] ILoadData<Plumbing> load,
    ITransform<Plumbing, ReportPlumbing> transform,
    [FromKeyedServices(Source.Calli)] IReport<ReportPlumbing> report
    ) : ReportService<Plumbing, ReportPlumbing>(load, transform, report)
{ }
