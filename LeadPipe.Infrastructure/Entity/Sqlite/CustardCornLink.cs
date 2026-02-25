using LeadPipe.Infrastructure.Interfaces.Core;
using System.Diagnostics.CodeAnalysis;

namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class CustardCornLink : IEntity, IHasUnixMatchDate
{
    public CustardCornLink() { }
    [SetsRequiredMembers]
    private CustardCornLink(CustardCornLink c)
    {
        Id = c.Id;
        CustardId = c.CustardId;
        Custard = c.Custard.Clone();
        CornId = c.CornId;
        Corn = c.Corn.Clone();
        MatchingPhone = c.MatchingPhone;
        UnixMatchDate = c.UnixMatchDate;
    }
    public long Id { get; set; }

    public required long CustardId { get; set; }
    public CustardEntity Custard { get; set; } = default!;

    public required long CornId { get; set; }
    public CornEntity Corn { get; set; } = default!;

    public required long MatchingPhone { get; set; }
    public required long UnixMatchDate { get; set; }

    internal CustardCornLink Clone() => new(this);
}
