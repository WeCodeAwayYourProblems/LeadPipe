namespace LeadPipe.Infrastructure.Entity.Sqlite;

/// <summary>
/// Denormalized link for fast Sand-based access.
/// Canonical relationship is Sand -> Custard -> X.
/// </summary>
public class SandPlumbingLink : IEntity
{
    public long Id { get; set; }

    public long SandId { get; set; }
    public SandEntity? SandEntity { get; set; }

    public long PlumbingId { get; set; }
    public PlumbingEntity? PlumbingEntity { get; set; }

    public long MatchingPhone { get; set; }
}
