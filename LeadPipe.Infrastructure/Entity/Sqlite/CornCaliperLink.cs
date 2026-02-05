namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class CornCaliperLink : IEntity
{
    public long Id { get; set; }

    public required long CornId { get; set; }
    public CornEntity CornEntity { get; set; } = default!;

    public required long CaliperId { get; set; }
    public CaliperEntity CaliperEntity { get; set; } = default!;

    public required long MatchingPhone { get; set; }
    public required long UnixMatchDate { get; set; }
}
