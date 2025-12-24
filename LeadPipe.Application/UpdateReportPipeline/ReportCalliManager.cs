using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application.UpdateReportPipeline;

internal interface IReportCalliManager : IReportManager<Plumbing> { }
internal sealed class ReportCalliManager([FromKeyedServices(Source.Calli)] IReportService<Plumbing> report) : ReportManager<Plumbing>(report), IReportCalliManager { }
