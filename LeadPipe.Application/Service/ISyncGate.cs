using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Application.Service;

public interface ISyncGate
{
    Task<bool> ShouldRunAsync(Source source, SyncKey key);
    Task<bool> ShouldRunAsync(SyncKey key);
    Task MarkSuccessAsync(Source source, SyncKey key);
    Task MarkSuccessAsync(SyncKey key);
    Task MarkFailureAsync(Source source, SyncKey key, string error);
    Task MarkFailureAsync(SyncKey key, string error);
}
