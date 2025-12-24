using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Application.UpdateReportPipeline;

internal interface IUpdateSandwichManager : IUpdateManager<Sandwich> { }
internal sealed class UpdateSandwichManager(IUpdateService<Sandwich> update) : UpdateManager<Sandwich>(update), IUpdateSandwichManager { }