namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class CustardCaliperLink : IEntity
{
    public long Id { get; set; }

    public required long CustardId { get; set; }
    public CustardEntity Custard { get; set; } = default!;

    public required long CaliperId { get; set; }
    public CaliperEntity Caliper { get; set; } = default!;

    public required long MatchingPhone { get; set; }
    public required long UnixMatchDate { get; set; }
}
