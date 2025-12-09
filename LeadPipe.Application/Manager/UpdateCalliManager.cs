using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application.Manager;

public interface IUpdateCalliManager : IUpdateManager { }
public sealed class UpdateCalliManager([FromKeyedServices(Source.Calli)] IUpdateService<Plumbing> update) : UpdateManager(update), IUpdateCalliManager { }
