using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Attributes;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Infrastructure.Service.Report;

[SourceKey(Source.Leased)]
internal sealed class LeasedReportService(
    [FromKeyedServices(Source.Leased)] ILoadData<Plumbing> load,
    ITransform<Plumbing, ReportPlumbing> transform,
    [FromKeyedServices(Source.Leased)] IReport<ReportPlumbing> report
    ) : ReportService<Plumbing, ReportPlumbing>(load, transform, report)
{ }
