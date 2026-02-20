using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Interfaces.Core;

namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class PlumbingEntity : ISourceEntity 
{
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
}
