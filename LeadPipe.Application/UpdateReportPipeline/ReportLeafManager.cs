using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application.UpdateReportPipeline;

internal interface IReportLeafManager : IReportManager<Plumbing> { }
internal sealed class ReportLeafManager([FromKeyedServices(Source.Leaf)] IReportService<Plumbing> report) : ReportManager<Plumbing>(report), IReportLeafManager { }
