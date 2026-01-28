using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Application.Service;

public interface ISyncGate
{
    Task<bool> ShouldRunAsync(Source source, string entity);
    Task<bool> ShouldRunAsync(string entity);
    Task MarkSuccessAsync(Source source, string entity);
    Task MarkSuccessAsync(string entity);
    Task MarkFailureAsync(Source source, string entity, string error);
    Task MarkFailureAsync(string entity, string error);
}
