using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application.Manager;

public interface IReportCalliManager : IReportManager<Plumbing> { }
public sealed class ReportCalliManager([FromKeyedServices(Source.Calli)] IReportService<Plumbing> report) : ReportManager<Plumbing>(report), IReportCalliManager { }
