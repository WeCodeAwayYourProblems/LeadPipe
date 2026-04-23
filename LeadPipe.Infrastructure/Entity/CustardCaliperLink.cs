using LeadPipe.Infrastructure.Interfaces.Core;
using System.Diagnostics.CodeAnalysis;

namespace LeadPipe.Infrastructure.Entity;

public class CustardCaliperLink : IEntity, IHasUnixMatchDate
{
    public CustardCaliperLink() { }
    [SetsRequiredMembers]
    private CustardCaliperLink(CustardCaliperLink l)
    {
        Id = l.Id;
        CustardId = l.CustardId;
        Custard = l.Custard.Clone();
        CaliperId = l.CaliperId;
        Caliper = l.Caliper.Clone();
        MatchingPhone = l.MatchingPhone;
        UnixMatchDate = l.UnixMatchDate;
    }
    public long Id { get; set; }

    public required long CustardId { get; set; }
    public CustardEntity Custard { get; set; } = default!;

    public required long CaliperId { get; set; }
    public CaliperEntity Caliper { get; set; } = default!;

    public required long MatchingPhone { get; set; }
    public required long UnixMatchDate { get; set; }

    internal CustardCaliperLink Clone() => new(this);
}
