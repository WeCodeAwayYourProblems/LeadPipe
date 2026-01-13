namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class CustardPlumbingLink : IEntity
{
    public long Id { get; set; }

    public long CustardId { get; set; }
    public CustardEntity Custard { get; set; } = default!;

    public long PlumbingId { get; set; }
    public PlumbingEntity Plumbing { get; set; } = default!;

    public long MatchingPhone { get; set; }
}
