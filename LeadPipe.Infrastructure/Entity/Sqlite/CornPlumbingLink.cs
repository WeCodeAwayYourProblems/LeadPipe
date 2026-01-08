namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class CornPlumbingLink:IEntity
{
    public long Id { get; set; }

    public long CornId { get; set; }
    public CornEntity CornEntity { get; set; } = default!;

    public long PlumbingId { get; set; }
    public PlumbingEntity PlumbingEntity { get; set; } = default!;

    public long MatchingPhone { get; set; }
}
