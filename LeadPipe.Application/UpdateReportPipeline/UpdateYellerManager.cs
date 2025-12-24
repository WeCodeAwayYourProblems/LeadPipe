using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application.UpdateReportPipeline;

internal interface IUpdateYellerManager : IUpdateManager<Plumbing> { }
internal sealed class UpdateYellerManager([FromKeyedServices(Source.Yeller)] IUpdateService<Plumbing> update) : UpdateManager<Plumbing>(update), IUpdateYellerManager { }
