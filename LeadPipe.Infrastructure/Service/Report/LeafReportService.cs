using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Attributes;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Infrastructure.Service.Report;

[SourceKey(Source.Leaf)]
internal sealed class LeafReportService(
    [FromKeyedServices(Source.Leaf)] ILoadData<Plumbing> load,
    ITransform<Plumbing, ReportPlumbing> transform,
    [FromKeyedServices(Source.Leaf)] IReport<ReportPlumbing> report
    ) : ReportService<Plumbing, ReportPlumbing>(load, transform, report)
{ }
