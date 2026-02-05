namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class CustardCornLink : IEntity
{
    public long Id { get; set; }

    public required long CustardId { get; set; }
    public CustardEntity Custard { get; set; } = default!;

    public required long CornId { get; set; }
    public CornEntity Corn { get; set; } = default!;

    public required long MatchingPhone { get; set; }
    public required long UnixMatchDate { get; set; }
}
