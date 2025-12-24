using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application.UpdateReportPipeline;

internal interface IReportPanManager : IReportManager<Plumbing> { }
internal sealed class ReportPanManager([FromKeyedServices(Source.Pan)] IReportService<Plumbing> report) : ReportManager<Plumbing>(report), IReportPanManager { }
