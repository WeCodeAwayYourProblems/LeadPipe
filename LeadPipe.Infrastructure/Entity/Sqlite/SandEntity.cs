namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class SandEntity : IEntity
{
    public required long Id { get; set; }
    public long CustomerId { get; set; }
    public DateTime Date { get; set; }
    public long UnixDate { get; set; }
    public DateTime SubDate { get; set; }
    public long UnixSubDate { get; set; }
    public long PhoneNumber { get; set; }
    public long PhoneNumber2 { get; set; }
    public DateTime CancelDate { get; set; }
    public long UnixCancelDate { get; set; }
    public DateTime SubCancelDate { get; set; }
    public long UnixSubCancelDate { get; set; }
    public bool Active { get; set; }
    public bool SubActive { get; set; }
    public bool Complete { get; set; }
    public decimal Value { get; set; }
    public string? Type { get; set; }
    public int Seller { get; set; }
    public int Seller2 { get; set; }
    public int Seller3 { get; set; }

    // Navigation properties
    public ICollection<SandPlumbingLink> SandPlumbingLinks { get; set; } = [];
    public ICollection<SandCaliperLink> SandCaliperLinks { get; set; } = [];
    public ICollection<SandCornLink> SandCornLinks { get; set; } = [];
}
