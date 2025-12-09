using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application.Manager;

public interface IReportLeafManager : IReportManager<Plumbing> { }
public sealed class ReportLeafManager([FromKeyedServices(Source.Leaf)] IReportService<Plumbing> report) : ReportManager<Plumbing>(report), IReportLeafManager { }
