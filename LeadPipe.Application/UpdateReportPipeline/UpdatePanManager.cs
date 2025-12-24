using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application.UpdateReportPipeline;

internal interface IUpdatePanManager : IUpdateManager<Plumbing> { }
internal sealed class UpdatePanManager([FromKeyedServices(Source.Pan)] IUpdateService<Plumbing> update) : UpdateManager<Plumbing>(update), IUpdatePanManager { }
