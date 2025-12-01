using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class PlumbingEntity : IEntity
{
    public long Id { get; set; }
    public long PhoneNumber { get; set; }
    public DateTime Date { get; set; }
    public long UnixDate { get; set; }
    public string? Contents { get; set; }
    public Source Source { get; set; }

    public ICollection<SubsPlumbingLink> SubsPlumbingLinks { get; set; } = [];
    public ICollection<PlumbingCallLink> PlumbingCallLinks { get; set; } = [];
}
