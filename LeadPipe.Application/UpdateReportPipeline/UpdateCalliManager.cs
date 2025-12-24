using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application.UpdateReportPipeline;

internal interface IUpdateCalliManager : IUpdateManager<Plumbing> { }
internal sealed class UpdateCalliManager([FromKeyedServices(Source.Calli)] IUpdateService<Plumbing> update) : UpdateManager<Plumbing>(update), IUpdateCalliManager { }
