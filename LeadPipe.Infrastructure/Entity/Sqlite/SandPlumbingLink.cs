using LeadPipe.Infrastructure.Interfaces.Core;
using System.Diagnostics.CodeAnalysis;

namespace LeadPipe.Infrastructure.Entity.Sqlite;

/// <summary>
/// Denormalized link for fast Sand-based access.
/// Canonical relationship is Sand -> Custard -> X.
/// </summary>
public class SandPlumbingLink : IEntity, IHasUnixMatchDate
{
    public SandPlumbingLink() { }
    [SetsRequiredMembers]
    private SandPlumbingLink(SandPlumbingLink s)
    {
        Id = s.Id;
        SandId = s.SandId;
        SandEntity = s.SandEntity?.Clone();
        PlumbingId = s.PlumbingId;
        PlumbingEntity = s.PlumbingEntity?.Clone();
        MatchingPhone = s.MatchingPhone;
        UnixMatchDate = s.UnixMatchDate;

    }
    public long Id { get; set; }

    public required long SandId { get; set; }
    public SandEntity? SandEntity { get; set; }

    public required long PlumbingId { get; set; }
    public PlumbingEntity? PlumbingEntity { get; set; }

    public required long MatchingPhone { get; set; }
    public required long UnixMatchDate { get; set; }

    internal SandPlumbingLink Clone() => new(this);
}
