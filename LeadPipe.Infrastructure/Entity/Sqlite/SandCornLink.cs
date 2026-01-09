namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class SandCornLink : IEntity
{
    public long Id { get; set; }

    public long SandId { get; set; }
    public SandEntity SandEntity { get; set; } = default!;

    public long CornId { get; set; }
    public CornEntity CornEntity { get; set; } = default!;

    public long MatchingPhone { get; set; }
}
