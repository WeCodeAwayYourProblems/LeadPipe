namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class CustardPlumbingLink : IEntity
{
    public long Id { get; set; }

    public required long CustardId { get; set; }
    public CustardEntity Custard { get; set; } = default!;

    public required long PlumbingId { get; set; }
    public PlumbingEntity Plumbing { get; set; } = default!;

    public required long MatchingPhone { get; set; }
    public required long UnixMatchDate { get; set; }
}
