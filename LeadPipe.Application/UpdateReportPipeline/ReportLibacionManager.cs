using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application.UpdateReportPipeline;

internal interface IReportLibacionManager : IReportManager<Plumbing> { }
internal sealed class ReportLibacionManager([FromKeyedServices(Source.Libacion)] IReportService<Plumbing> report) : ReportManager<Plumbing>(report), IReportLibacionManager { }
