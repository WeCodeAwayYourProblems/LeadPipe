namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class CaliperEntity : IEntity
{
    public required long Id { get; set; }
    public long PhoneNumber { get; set; }
    public DateTime CaliperDate { get; set; }
    public long UnixDate { get; set; }
    public required string Note { get; set; }
    public required string Source { get; set; }
    public required string Location { get; set; }
    public long Duration { get; set; }
    public bool Billable { get; set; }

    public ICollection<CustardCaliperLink> CustardCaliperLinks { get; set; } = [];
    public ICollection<SandCaliperLink> SandCaliperLinks { get; set; } = [];
    public ICollection<PlumbingCaliperLink> PlumbingCaliperLinks { get; set; } = [];
    public ICollection<CornCaliperLink> CornCaliperLinks { get; set; } = [];
}
