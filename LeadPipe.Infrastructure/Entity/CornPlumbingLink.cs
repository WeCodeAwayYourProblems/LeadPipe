using LeadPipe.Infrastructure.Interfaces.Core;
using System.Diagnostics.CodeAnalysis;

namespace LeadPipe.Infrastructure.Entity;

public class CornPlumbingLink : IEntity, IHasUnixMatchDate
{
    public CornPlumbingLink() { }
    [SetsRequiredMembers]
    private CornPlumbingLink(CornPlumbingLink c)
    {
        Id = c.Id;
        CornEntity = c.CornEntity.Clone();
        PlumbingId = c.PlumbingId;
        PlumbingEntity = c.PlumbingEntity.Clone();
        MatchingPhone = c.MatchingPhone;
        UnixMatchDate = c.UnixMatchDate;
    }
    public long Id { get; set; }

    public required long CornId { get; set; }
    public CornEntity CornEntity { get; set; } = default!;

    public required long PlumbingId { get; set; }
    public PlumbingEntity PlumbingEntity { get; set; } = default!;

    public required long MatchingPhone { get; set; }
    public required long UnixMatchDate { get; set; }

    internal CornPlumbingLink Clone() => new(this);
}
