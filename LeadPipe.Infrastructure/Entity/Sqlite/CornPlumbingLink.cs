namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class CornPlumbingLink:IEntity
{
    public long Id { get; set; }

    public required long CornId { get; set; }
    public CornEntity CornEntity { get; set; } = default!;

    public required long PlumbingId { get; set; }
    public PlumbingEntity PlumbingEntity { get; set; } = default!;

    public required long MatchingPhone { get; set; }
    public required long UnixMatchDate { get; set; }
}
