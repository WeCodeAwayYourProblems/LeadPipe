using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class PlumbingEntity : ISourceEntity
{
    public required long Id { get; set; }
    public long PhoneNumber { get; set; }
    public DateTime Date { get; set; }
    public long UnixDate { get; set; }
    public string? Contents { get; set; }
    public Source Source { get; set; }
    public required string MetaData { get; set; }

    public ICollection<CustardPlumbingLink> CustardPlumbingLinks { get; set; } = [];
    public ICollection<SandPlumbingLink> SandPlumbingLinks { get; set; } = [];
    public ICollection<PlumbingCaliperLink> PlumbingCaliperLinks { get; set; } = [];
    public ICollection<CornPlumbingLink> CornPlumbingLinks { get; set; } = [];
}
