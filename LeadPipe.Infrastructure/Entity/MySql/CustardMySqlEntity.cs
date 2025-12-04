namespace LeadPipe.Infrastructure.Entity.MySql;
public class CustardMySqlEntity
{
#pragma warning disable IDE1006 // Naming Styles
    public int customerID { get; set; }//c
    public int status { get; set; }//c
    public string? phone1 { get; set; }//c
    public string? phone2 { get; set; }//c
    public DateTime dateAdded { get; set; }//c
    public DateTime dateCancelled { get; set; }//c
#pragma warning restore IDE1006 // Naming Styles
}
