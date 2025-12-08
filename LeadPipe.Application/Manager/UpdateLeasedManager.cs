using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application.Manager;

public interface IUpdateLeasedManager : IUpdateManager { }
public sealed class UpdateLeasedManager([FromKeyedServices(Source.Leased)] IUpdateService<Plumbing> update) : UpdateManager(update), IUpdateLeasedManager { }
