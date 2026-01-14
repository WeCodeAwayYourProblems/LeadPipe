using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Infrastructure.Entity.Sqlite;

// Deduplication is based on PhoneNumber and Date, not PhoneNumber, Date, and Source. That way, there is one Source that stands as the initial contact point
public class PlumbingEntity : ISourceEntity 
{
    public required long Id { get; set; }
    public long PhoneNumber { get; set; }
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
}
