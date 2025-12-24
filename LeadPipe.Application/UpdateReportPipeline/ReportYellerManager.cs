using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application.UpdateReportPipeline;

internal interface IReportYellerManager : IReportManager<Plumbing> { }
internal sealed class ReportYellerManager([FromKeyedServices(Source.Yeller)] IReportService<Plumbing> report) : ReportManager<Plumbing>(report), IReportYellerManager { }
