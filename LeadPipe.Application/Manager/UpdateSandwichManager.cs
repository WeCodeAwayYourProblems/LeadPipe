using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Application.Manager;

public interface IUpdateSandwichManager : IUpdateManager<Sandwich> { }
public sealed class UpdateSandwichManager(IUpdateService<Sandwich> update) : UpdateManager<Sandwich>(update), IUpdateSandwichManager { }