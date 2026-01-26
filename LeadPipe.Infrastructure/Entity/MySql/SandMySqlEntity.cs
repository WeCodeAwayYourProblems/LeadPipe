namespace LeadPipe.Infrastructure.Entity.MySql;

public class SandMySqlEntity
{
#pragma warning disable IDE1006 // Naming Styles
    public int subscriptionID { get; set; }
    public int customerID { get; set; }
    public DateTime? dateAdded { get; set; }
    public int? active { get; set; }
    public decimal contractValue { get; set; }
    public string? serviceType { get; set; }
    public int? initialStatus { get; set; }
    public DateTime? dateCancelled { get; set; }
    public int? soldBy { get; set; }
    public int? soldBy2 { get; set; }
    public int? soldBy3 { get; set; }
    public int officeID { get; set; }


    // Navigation Property
    public CustardMySqlEntity? customer { get; set; }
    public OffermanMySqlEntity? offerman { get; set; }

#pragma warning restore IDE1006 // Naming Styles
}
