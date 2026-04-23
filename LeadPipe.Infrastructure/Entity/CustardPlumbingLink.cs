using LeadPipe.Infrastructure.Interfaces.Core;
using System.Diagnostics.CodeAnalysis;

namespace LeadPipe.Infrastructure.Entity;

public class CustardPlumbingLink : IEntity, IHasUnixMatchDate
{
    public CustardPlumbingLink() { }
    [SetsRequiredMembers]
    private CustardPlumbingLink(CustardPlumbingLink c)
    {
        Id = c.Id;
        CustardId = c.CustardId;
        Custard = c.Custard.Clone();
        PlumbingId = c.PlumbingId;
        Plumbing = c.Plumbing.Clone();
        MatchingPhone = c.MatchingPhone;
        UnixMatchDate = c.UnixMatchDate;
    }

    public long Id { get; set; }

    public required long CustardId { get; set; }
    public CustardEntity Custard { get; set; } = default!;

    public required long PlumbingId { get; set; }
    public PlumbingEntity Plumbing { get; set; } = default!;

    public required long MatchingPhone { get; set; }
    public required long UnixMatchDate { get; set; }

    internal CustardPlumbingLink Clone() => new(this);
}
