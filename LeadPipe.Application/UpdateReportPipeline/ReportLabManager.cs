using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application.UpdateReportPipeline;

internal interface IReportLabManager : IReportManager<Plumbing> { }
internal sealed class ReportLabManager([FromKeyedServices(Source.Lab)] IReportService<Plumbing> report) : ReportManager<Plumbing>(report), IReportLabManager { }
