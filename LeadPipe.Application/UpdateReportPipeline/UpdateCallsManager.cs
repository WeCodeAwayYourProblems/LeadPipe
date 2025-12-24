using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Application.UpdateReportPipeline;

internal interface IUpdateCallsManager : IUpdateManager<Call> { }
internal sealed class UpdateCallsManager(IUpdateService<Call> update) : UpdateManager<Call>(update), IUpdateCallsManager { }
