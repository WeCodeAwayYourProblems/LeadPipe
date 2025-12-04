namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class SubsEntity : IEntity
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public DateTime Date { get; set; }
    public long UnixDate { get; set; }
    public DateTime SubDate { get; set; }
    public long UnixSubDate { get; set; }
    public long Number { get; set; }
    public long Number2 { get; set; }
    public DateTime CancelDate { get; set; }
    public long UnixCancelDate { get; set; }
    public DateTime SubCancelDate { get; set; }
    public long UnixSubCancelDate { get; set; }
    public bool Active { get; set; }
    public bool SubActive { get; set; }
    public bool Complete { get; set; }
    public double Value { get; set; }
    public string? Seller { get; set; }
    public string? Seller2 { get; set; }
    public string? Seller3 { get; set; }

    // Navigation properties
    public ICollection<SubsPlumbingLink> SubsPlumbingLinks { get; set; } = [];
    public ICollection<SubsCallLink> SubsCallLinks { get; set; } = [];
}
