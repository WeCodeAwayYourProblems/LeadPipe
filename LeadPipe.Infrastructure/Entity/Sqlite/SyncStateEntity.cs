namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class SyncStateEntity
{
    public int Id { get; private set; } = 1;
    public string? LastProcessedId { get; set; }
    public DateTime LastSyncUtc { get; set; }
    public long UnixLastSyncUtc { get; set; }
}
