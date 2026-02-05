namespace LeadPipe.Infrastructure.Entity.Sqlite;

/// <summary>
/// Denormalized link for fast Sand-based access.
/// Canonical relationship is Sand -> Custard -> X.
/// </summary>
public class SandCornLink : IEntity
{
    public long Id { get; set; }

    public required long SandId { get; set; }
    public SandEntity SandEntity { get; set; } = default!;

    public required long CornId { get; set; }
    public CornEntity CornEntity { get; set; } = default!;

    public required long MatchingPhone { get; set; }
    public required long UnixMatchDate { get; set; }
}
