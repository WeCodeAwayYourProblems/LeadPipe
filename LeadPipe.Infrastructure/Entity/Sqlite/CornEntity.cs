using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class CornEntity : ISourceEntity
{
    public long Id { get; set; }
    public long PhoneNumber { get; set; }
    public DateTime Date { get; set; }
    public long UnixDate { get; set; }
    public string Payload { get; set; } = default!;
    public string MetaData { get; set; } = default!;
    public Source Source { get; set; }

    public ICollection<SubsCornLink> SubsCornLinks { get; set; } = [];
    public ICollection<CornCallLink> CornCallLinks { get; set; } = [];
    public ICollection<CornPlumbingLink> CornPlumbingLinks { get; set; } = [];
}
