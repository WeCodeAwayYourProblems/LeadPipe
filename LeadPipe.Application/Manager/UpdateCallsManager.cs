using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Application.Manager;

public interface IUpdateCallsManager : IUpdateManager<Call> { }
public sealed class UpdateCallsManager(IUpdateService<Call> update) : UpdateManager<Call>(update), IUpdateCallsManager { }
