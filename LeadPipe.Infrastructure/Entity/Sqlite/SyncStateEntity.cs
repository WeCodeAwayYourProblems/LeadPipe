using CSharpFunctionalExtensions;

namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class SyncStateEntity
{
    public int Id { get; set; }
    public required BusinessId BusinessId { get; set; }
    public string? LastProcessedId { get; set; }
    public DateTime LastSyncUtc { get; set; }
    public long UnixLastSyncUtc { get; set; }
}

public readonly record struct BusinessId(string Value)
{
    public string Value { get; } = Value;
    public static BusinessId From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("BusinessId cannot be empty");

        return new BusinessId(value);
    }

    public override string ToString() => Value;
}
