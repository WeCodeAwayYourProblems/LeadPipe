namespace LeadPipe.Infrastructure.Entity;

public class CustardMySqlEntity
{
#pragma warning disable IDE1006 // Naming Styles
    public int customerID { get; set; }
    public int status { get; set; }
    public string? phone1 { get; set; }
    public string? phone2 { get; set; }
    public DateTime? dateAdded { get; set; }
    public DateTime? dateCancelled { get; set; }

    public ICollection<SandMySqlEntity> subscriptions { get; set; } = [];
#pragma warning restore IDE1006 // Naming Styles
}
