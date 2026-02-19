using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Interfaces.Core;

namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class CustardEntity : IEntity
{
    public required long Id { get; set; }
    public bool Active { get; set; }
    public required PhoneNumber PhoneNumber { get; set; }
    public PhoneNumber? PhoneNumber2 { get; set; }
    public DateTime Date { get; set; }
    public required long UnixDate { get; set; }
    public DateTime CancelDate { get; set; }
    public long UnixCancelDate { get; set; }

    // Navigation    
    public ICollection<SandEntity> SandEntities { get; set; } = [];
    public ICollection<CustardCaliperLink> CustardCaliperLinks { get; set; } = [];
    public ICollection<CustardCornLink> CustardCornLinks { get; set; } = [];
    public ICollection<CustardPlumbingLink> CustardPlumbingLinks { get; set; } = [];
}
