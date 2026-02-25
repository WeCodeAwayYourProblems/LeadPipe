using LeadPipe.Infrastructure.Interfaces.Core;
using System.Diagnostics.CodeAnalysis;

namespace LeadPipe.Infrastructure.Entity.Sqlite;

/// <summary>
/// Denormalized link for fast Sand-based access.
/// Canonical relationship is Sand -> Custard -> X.
/// </summary>
public class SandCornLink : IEntity, IHasUnixMatchDate
{
    public SandCornLink() { }
    [SetsRequiredMembers]
    private SandCornLink(SandCornLink s)
    {
        Id = s.Id;
        SandId = s.SandId;
        SandEntity = s.SandEntity.Clone();
        CornId = s.CornId;
        CornEntity = s.CornEntity.Clone();
        MatchingPhone = s.MatchingPhone;
        UnixMatchDate = s.UnixMatchDate;
    }
    public long Id { get; set; }

    public required long SandId { get; set; }
    public SandEntity SandEntity { get; set; } = default!;

    public required long CornId { get; set; }
    public CornEntity CornEntity { get; set; } = default!;

    public required long MatchingPhone { get; set; }
    public required long UnixMatchDate { get; set; }

    internal SandCornLink Clone() => new(this);
}
