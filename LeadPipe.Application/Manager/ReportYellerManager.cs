using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application.Manager;

public interface IReportYellerManager : IReportManager<Plumbing> { }
public sealed class ReportYellerManager([FromKeyedServices(Source.Yeller)] IReportService<Plumbing> report) : ReportManager<Plumbing>(report), IReportYellerManager { }
