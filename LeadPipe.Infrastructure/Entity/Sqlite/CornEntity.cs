using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class CornEntity : ISourceEntity
{
    public required long Id { get; set; }
    public required long PhoneNumber { get; set; }
    public required DateTime Date { get; set; }
    public required long UnixDate { get; set; }
    public required string Payload { get; set; }
    public required string MetaData { get; set; }
    public required Source Source { get; set; }

    public ICollection<SandCornLink> SandCornLinks { get; set; } = [];
    public ICollection<CornCaliperLink> CornCaliperLinks { get; set; } = [];
    public ICollection<CornPlumbingLink> CornPlumbingLinks { get; set; } = [];
}
