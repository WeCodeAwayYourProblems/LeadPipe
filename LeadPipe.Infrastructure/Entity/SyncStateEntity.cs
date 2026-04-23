using LeadPipe.Infrastructure.Interfaces.Core;

namespace LeadPipe.Infrastructure.Entity;

public class SyncStateEntity : IEntity
{
    public long Id { get; set; }
    public required BusinessId BusinessId { get; set; }
    public string? LastProcessedId { get; set; }
    public DateTime LastSyncUtc { get; set; }
    public long UnixLastSyncUtc { get; set; }
}
