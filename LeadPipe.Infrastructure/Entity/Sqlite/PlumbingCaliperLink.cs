using LeadPipe.Infrastructure.Interfaces.Core;
using System.Diagnostics.CodeAnalysis;

namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class PlumbingCaliperLink : IEntity, IHasUnixMatchDate
{
    public PlumbingCaliperLink() { }
    [SetsRequiredMembers]
    private PlumbingCaliperLink(PlumbingCaliperLink p)
    {
        Id = p.Id;
        PlumbingId = p.PlumbingId;
        PlumbingEntity = p.PlumbingEntity?.Clone();
        CaliperId = p.CaliperId;
        CaliperEntity = p.CaliperEntity?.Clone();
        MatchingPhone = p.MatchingPhone;
        UnixMatchDate = p.UnixMatchDate;
    }

    public long Id { get; set; }

    public required long PlumbingId { get; set; }
    public PlumbingEntity? PlumbingEntity { get; set; }

    public required long CaliperId { get; set; }
    public CaliperEntity? CaliperEntity { get; set; }

    public required long MatchingPhone { get; set; }
    public required long UnixMatchDate { get; set; }

    internal PlumbingCaliperLink Clone() => new(this);
}
