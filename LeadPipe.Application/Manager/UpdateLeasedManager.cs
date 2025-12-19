using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Application.Manager;

public interface IUpdateLeasedManager : IUpdateManager<Plumbing> { }
public sealed class UpdateLeasedManager([FromKeyedServices(Source.Leased)] IUpdateService<Plumbing> update) : UpdateManager<Plumbing>(update), IUpdateLeasedManager { }
