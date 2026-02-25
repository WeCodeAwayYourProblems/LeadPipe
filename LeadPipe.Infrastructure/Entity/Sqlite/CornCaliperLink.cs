using LeadPipe.Infrastructure.Interfaces.Core;
using System.Diagnostics.CodeAnalysis;

namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class CornCaliperLink : IEntity, IHasUnixMatchDate
{
    public CornCaliperLink() { }
    [SetsRequiredMembers]
    private CornCaliperLink(CornCaliperLink c)
    {
        Id = c.Id;
        CornId = c.CornId;
        CornEntity = c.CornEntity.Clone();
        CaliperId = c.CaliperId;
        CaliperEntity = c.CaliperEntity.Clone();
        MatchingPhone = c.MatchingPhone;
        UnixMatchDate = c.UnixMatchDate;
    }
    public long Id { get; set; }

    public required long CornId { get; set; }
    public CornEntity CornEntity { get; set; } = default!;

    public required long CaliperId { get; set; }
    public CaliperEntity CaliperEntity { get; set; } = default!;

    public required long MatchingPhone { get; set; }
    public required long UnixMatchDate { get; set; }

    internal CornCaliperLink Clone() => new(this);
}
