using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application.UpdateReportPipeline;

internal interface IUpdateLeasedManager : IUpdateManager<Plumbing> { }
internal sealed class UpdateLeasedManager([FromKeyedServices(Source.Leased)] IUpdateService<Plumbing> update) : UpdateManager<Plumbing>(update), IUpdateLeasedManager { }
