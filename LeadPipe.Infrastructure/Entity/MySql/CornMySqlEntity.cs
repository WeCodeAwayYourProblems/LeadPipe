namespace LeadPipe.Infrastructure.Entity.MySql;

public class CornMySqlEntity
{
#pragma warning disable IDE1006
    public int id { get; set; }
    public string? firstName { get; set; }
    public string? lastName { get; set; }
    public string? phoneNumber { get; set; }
    public string? email { get; set; }
    public string? zipcode { get; set; }
    public string? form { get; set; }
    public string? promo { get; set; }
    public DateTime timestamp { get; set; }
    public string? source { get; set; }
    public string? medium { get; set; }
    public string? campaign { get; set; }
    public string? formName { get; set; }
    public string? hearedAbout { get; set; }
    public string? referredBy { get; set; }
    public string? comments { get; set; }
    public string? currentCustomer { get; set; }
    public string? discontinueReason { get; set; }
    public string? addressLine1 { get; set; }
    public string? addressLine2 { get; set; }
    public string? city { get; set; }
    public string? state { get; set; }
    public string? howCanWeHelp { get; set; }
    public string? referringURL { get; set; }
    public string? contactAcceptance { get; set; }
    public bool commercial { get; set; }
#pragma warning restore IDE1006
}