namespace LeadPipe.Infrastructure.Entity.MySql;

public class CallMySqlEntity
{
#pragma warning disable IDE1006 // Naming Styles
    public long call_id { get; set; }//c
    public int duration { get; set; }//c
    public string? sale_billable { get; set; }//c
    public string? contact_number_clean { get; set; }//c
    public DateTime called_at_utc { get; set; }//c
#pragma warning restore IDE1006 // Naming Styles
}
