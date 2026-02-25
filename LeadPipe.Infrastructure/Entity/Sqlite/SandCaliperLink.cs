using LeadPipe.Infrastructure.Interfaces.Core;
using System.Diagnostics.CodeAnalysis;

namespace LeadPipe.Infrastructure.Entity.Sqlite;

/// <summary>
/// Denormalized link for fast Sand-based access.
/// Canonical relationship is Sand -> Custard -> X.
/// </summary>
public class SandCaliperLink : IEntity, IHasUnixMatchDate
{
    public SandCaliperLink() { }
    [SetsRequiredMembers]
    private SandCaliperLink(SandCaliperLink s)
    {
        Id = s.Id;
        SandId = s.SandId;
        SandEntity = s.SandEntity?.Clone();
        CaliperId = s.CaliperId;
        CaliperEntity = s.CaliperEntity?.Clone();
        MatchingPhone = s.MatchingPhone;
        UnixMatchDate = s.UnixMatchDate;
    }
    public long Id { get; set; }
    public required long SandId { get; set; }
    public SandEntity? SandEntity { get; set; }

    public required long CaliperId { get; set; }
    public CaliperEntity? CaliperEntity { get; set; }

    public required long MatchingPhone { get; set; }
    public required long UnixMatchDate { get; set; }

    internal SandCaliperLink Clone() => new(this);
}
