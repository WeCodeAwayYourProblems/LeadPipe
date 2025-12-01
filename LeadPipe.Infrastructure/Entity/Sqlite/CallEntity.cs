namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class CallEntity : IEntity
{
    public long Id { get; set; }
    public long PhoneNumber { get; set; }
    public DateTime CallDate { get; set; }
    public long UnixCallDate { get; set; }
    public required string Note { get; set; }
    public required string Source { get; set; }
    public long Duration { get; set; }
    public bool Billable { get; set; }

    public ICollection<SubsCallLink> SubsCallLinks { get; set; } = [];
    public ICollection<PlumbingCallLink> PlumbingCallLinks { get; set; } = [];
}
