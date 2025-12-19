using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application.Manager;

public interface IUpdateLibacionManager : IUpdateManager<Plumbing> { }
public sealed class UpdateLibacionManager([FromKeyedServices(Source.Libacion)] IUpdateService<Plumbing> update) : UpdateManager<Plumbing>(update), IUpdateLibacionManager { }
