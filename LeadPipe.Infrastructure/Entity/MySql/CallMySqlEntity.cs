namespace LeadPipe.Infrastructure.Entity.MySql;

public class CallMySqlEntity
{
#pragma warning disable IDE1006 // Naming Styles
    public long call_id { get; set; }
    public long account_id { get; set; }
    public string? name { get; set; }
    public DateTime called_at { get; set; }
    public string? contact_number { get; set; }
    public string? call_status { get; set; }
    public int duration { get; set; }
    public string? note { get; set; }
    public string? audio { get; set; }
    public int current_customer { get; set; }
    public string? location { get; set; }
    public string? referrer { get; set; }
    public string? campaign { get; set; }
    public string? city { get; set; }
    public string? state { get; set; }
    public string? country { get; set; }
    public string? sale_billable { get; set; }
    public string? sale_score { get; set; }
    public string? sale_conversion { get; set; }
    public string? sale_value { get; set; }
    public DateTime snapshotDate { get; set; }
    public DateTime dateContacted { get; set; }
    public string? source { get; set; }
    public int codeBlue { get; set; }
    public int codeRed { get; set; }
    public int codeGreen { get; set; }
    public string? contact_number_clean { get; set; }
    public int in_database { get; set; }
    public string? weekDay { get; set; }
    public string? time_zone { get; set; }
    public DateTime called_at_utc { get; set; }
    public DateTime called_at_denver { get; set; }
    public string? original_billable { get; set; }
    public string? tracking_number { get; set; }
    public string? numbers_name { get; set; }
    public int officeID { get; set; }
    public string? original_source { get; set; }
    public string? zip { get; set; }
    public string? branch { get; set; }
    public string? ml_billable { get; set; }
    public string? ml_billable_test { get; set; }
    public string? sale_billable_source { get; set; }
#pragma warning restore IDE1006 // Naming Styles
}
