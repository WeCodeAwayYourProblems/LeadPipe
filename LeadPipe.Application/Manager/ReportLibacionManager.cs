using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application.Manager;

public interface IReportLibacionManager : IReportManager<Plumbing> { }
public sealed class ReportLibacionManager([FromKeyedServices(Source.Libacion)] IReportService<Plumbing> report) : ReportManager<Plumbing>(report), IReportLibacionManager { }
