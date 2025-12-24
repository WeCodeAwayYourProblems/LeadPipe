using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application.UpdateReportPipeline;

internal interface IUpdateLibacionManager : IUpdateManager<Plumbing> { }
internal sealed class UpdateLibacionManager([FromKeyedServices(Source.Libacion)] IUpdateService<Plumbing> update) : UpdateManager<Plumbing>(update), IUpdateLibacionManager { }
