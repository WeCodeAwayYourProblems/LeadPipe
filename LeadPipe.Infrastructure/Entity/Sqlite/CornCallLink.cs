namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class CornCallLink
{
    public long Id { get; set; }

    public long CornId { get; set; }
    public CornEntity CornEntity { get; set; } = default!;

    public long CallId { get; set; }
    public CallEntity CallEntity { get; set; } = default!;

    public long MatchingPhone { get; set; }
}
