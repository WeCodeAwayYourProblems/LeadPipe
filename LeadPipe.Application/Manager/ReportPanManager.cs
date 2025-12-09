using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application.Manager;

public interface IReportPanManager : IReportManager<Plumbing> { }
public sealed class ReportPanManager([FromKeyedServices(Source.Pan)] IReportService<Plumbing> report) : ReportManager<Plumbing>(report), IReportPanManager { }
