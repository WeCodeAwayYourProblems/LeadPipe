namespace LeadPipe.Infrastructure.Entity.Sqlite;

/// <summary>
/// Denormalized link for fast Sand-based access.
/// Canonical relationship is Sand -> Custard -> X.
/// </summary>
public class SandCaliperLink : IEntity
{
    public long Id { get; set; }
    public long SandId { get; set; }
    public SandEntity? SandEntity { get; set; }

    public long CaliperId { get; set; }
    public CaliperEntity? CaliperEntity { get; set; }

    public long MatchingPhone { get; set; }
}
