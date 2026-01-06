using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Attributes;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Infrastructure.Service.Report;

[ScheduleKey(Schedule.Daily)]
internal sealed class YellerClientReportService(
    [FromKeyedServices(Source.Yeller)] ILoadData<Plumbing> load,
    ITransform<Plumbing, ReportYeller> transform,
    IReport<ReportYeller> report
    ) : ReportService<Plumbing, ReportYeller>(load, transform, report)
{ }
