namespace LeadPipe.Infrastructure.Entity.Sqlite;

public class SandEntity : IEntity
{
    public required long Id { get; set; }
    public required long CustardId { get; set; }
    public DateTime Date { get; set; }
    public long UnixDate { get; set; }
    public DateTime CancelDate { get; set; }
    public long UnixCancelDate { get; set; }
    public bool Active { get; set; }
    public bool Complete { get; set; }
    public decimal Value { get; set; }
    public string? Type { get; set; }
    public int Seller { get; set; }
    public int Seller2 { get; set; }
    public int Seller3 { get; set; }

    // Navigation
    public CustardEntity? CustardEntity { get; set; }
    public ICollection<SandPlumbingLink> SandPlumbingLinks { get; set; } = [];
    public ICollection<SandCaliperLink> SandCaliperLinks { get; set; } = [];
    public ICollection<SandCornLink> SandCornLinks { get; set; } = [];
}
