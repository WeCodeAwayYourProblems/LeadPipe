using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Interfaces.Core;
using System.Diagnostics.CodeAnalysis;

namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class PlumbingEntity : ISourceEntity, IPhoneDateIdEntity
{
    public PlumbingEntity() { }
    [SetsRequiredMembers]
    private PlumbingEntity(PlumbingEntity p)
    {
        Id = p.Id;
        PhoneNumber = p.PhoneNumber;
        Date = p.Date;
        UnixDate = p.UnixDate;
        Contents = p.Contents;
        Source = p.Source;
        MetaData = p.MetaData;
        Branch = p.Branch;
        CustardPlumbingLinks = [.. p.CustardPlumbingLinks.Select(c => c)];
        SandPlumbingLinks = [.. p.SandPlumbingLinks.Select(c => c)];
        PlumbingCaliperLinks = [.. p.PlumbingCaliperLinks.Select(c => c)];
        CornPlumbingLinks = [.. p.CornPlumbingLinks.Select(c => c)];

    }
    public required long Id { get; set; }
    public required PhoneNumber PhoneNumber { get; set; }
    public DateTime Date { get; set; }
    public long UnixDate { get; set; }
    public string? Contents { get; set; }
    public Source Source { get; set; }
    public required string MetaData { get; set; }
    public string? Branch { get; set; }

    public ICollection<CustardPlumbingLink> CustardPlumbingLinks { get; set; } = [];
    public ICollection<SandPlumbingLink> SandPlumbingLinks { get; set; } = [];
    public ICollection<PlumbingCaliperLink> PlumbingCaliperLinks { get; set; } = [];
    public ICollection<CornPlumbingLink> CornPlumbingLinks { get; set; } = [];

    internal PlumbingEntity Clone() => new(this);
}
