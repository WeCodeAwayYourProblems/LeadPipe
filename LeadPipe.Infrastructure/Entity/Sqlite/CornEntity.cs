namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class CornEntity : IEntity
{
    public required long Id { get; set; }
    public required long PhoneNumber { get; set; }
    public required DateTime Date { get; set; }
    public required long UnixDate { get; set; }
    public required string Payload { get; set; }
    public required string MetaData { get; set; }
    public required string Source { get; set; }

    public ICollection<CustardCornLink> CustardCornLinks { get; set; } = [];
    public ICollection<SandCornLink> SandCornLinks { get; set; } = [];
    public ICollection<CornCaliperLink> CornCaliperLinks { get; set; } = [];
    public ICollection<CornPlumbingLink> CornPlumbingLinks { get; set; } = [];
}
