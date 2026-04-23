namespace LeadPipe.Infrastructure.Entity;

public class CaliperMySqlEntity
{
#pragma warning disable IDE1006
    public long call_id { get; set; }
    public int? duration { get; set; }
    public string? sale_billable { get; set; }
    public string? contact_number_clean { get; set; }
    public string? source { get; set; }
    public string? location { get; set; }
    public string? note { get; set; }
    public string? numbers_name { get; set; }
    public DateTime called_at_utc { get; set; }

    // Navigation properties
    public ICollection<SummaryMySqlEntity> summaries { get; set; } = [];
    public ICollection<TranscriptionMySqlEntity> transcriptions { get; set; } = [];
#pragma warning restore IDE1006
}
