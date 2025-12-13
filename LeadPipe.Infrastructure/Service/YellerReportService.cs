using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Service;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Infrastructure.Service;

internal sealed class YellerReportService(
    [FromKeyedServices(Source.Yeller)] ILoadData<Plumbing> load,
    ITransform<Plumbing, YellerReport> transform,
    IReport<YellerReport> report
    ) : ReportService<Plumbing, YellerReport>(load, transform, report), IYellerReportService
{ }
