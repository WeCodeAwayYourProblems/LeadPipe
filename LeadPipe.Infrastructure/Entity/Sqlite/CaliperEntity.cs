using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Interfaces.Core;
using System.Diagnostics.CodeAnalysis;

namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class CaliperEntity : IEntity, IPhoneDateIdEntity
{
    public CaliperEntity() { }
    [SetsRequiredMembers]
    private CaliperEntity(CaliperEntity c)
    {
        Id = c.Id;
        PhoneNumber = c.PhoneNumber;
        Date = c.Date;
        UnixDate = c.UnixDate;
        Note = c.Note;
        Source = c.Source;
        Location = c.Location;
        Label = c.Label;
        Duration = c.Duration;
        Billable = c.Billable;

        CustardCaliperLinks = [.. c.CustardCaliperLinks.Select(c => c)];
        SandCaliperLinks = [.. c.SandCaliperLinks.Select(c => c)];
        PlumbingCaliperLinks = [.. c.PlumbingCaliperLinks.Select(c => c)];
        CornCaliperLinks = [.. c.CornCaliperLinks.Select(c => c)];

    }
    public required long Id { get; set; }
    public required PhoneNumber PhoneNumber { get; set; }
    public DateTime Date { get; set; }
    public long UnixDate { get; set; }
    public required string Note { get; set; }
    public required string Source { get; set; }
    public required string Location { get; set; }
    public required string Label { get; set; }
    public long Duration { get; set; }
    public bool Billable { get; set; }

    public ICollection<CustardCaliperLink> CustardCaliperLinks { get; set; } = [];
    public ICollection<SandCaliperLink> SandCaliperLinks { get; set; } = [];
    public ICollection<PlumbingCaliperLink> PlumbingCaliperLinks { get; set; } = [];
    public ICollection<CornCaliperLink> CornCaliperLinks { get; set; } = [];

    internal CaliperEntity Clone() => new(this);
}
