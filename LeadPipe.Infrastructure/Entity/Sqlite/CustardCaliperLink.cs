namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class CustardCaliperLink : IEntity
{
    public long Id { get; set; }

    public long CustardId { get; set; }
    public CustardEntity Custard { get; set; } = default!;

    public long CaliperId { get; set; }
    public CaliperEntity Caliper { get; set; } = default!;

    public long MatchingPhone { get; set; }
}
