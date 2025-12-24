using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application.UpdateReportPipeline;

internal interface IReportLeasedManager: IReportManager<Plumbing> { }
internal sealed class ReportLeasedManager([FromKeyedServices(Source.Leased)] IReportService<Plumbing> report) : ReportManager<Plumbing>(report), IReportLeasedManager { }
