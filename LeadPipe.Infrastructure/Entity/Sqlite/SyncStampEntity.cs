using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Interfaces.Core;

namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class SyncStampEntity : IEntity
{
    public long Id { get; set; }
    public required SyncKey Key { get; set; }
    public Source? Source { get; set; }
    public long UnixSyncUtc { get; set; }
    public bool SuccessState { get; set; }
}