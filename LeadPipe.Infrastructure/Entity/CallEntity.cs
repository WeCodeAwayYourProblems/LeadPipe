namespace LeadPipe.Infrastructure.Entity;

public class CallEntity : IEntity
{
    public long Id { get; set; }
    public long PhoneNumber { get; set; }
    public DateTime CallDate { get; set; }
    public long UnixCallDate { get; set; }

    public ICollection<SubsCallLink> SubsCallLinks { get; set; } = [];
    public ICollection<PlumbingCallLink> PlumbingCallLinks { get; set; } = [];
}
