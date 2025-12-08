using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application.Manager;

public interface IUpdatePanManager : IUpdateManager { }
public sealed class UpdatePanManager([FromKeyedServices(Source.Pan)] IUpdateService<Plumbing> update) : UpdateManager(update), IUpdatePanManager { }
