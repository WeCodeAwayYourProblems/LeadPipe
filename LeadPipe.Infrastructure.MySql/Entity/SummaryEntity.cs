namespace LeadPipe.Infrastructure.MySql.Entity;

internal class SummaryEntity
{
#pragma warning disable IDE1006 // Naming Styles
    public long call_id { get; set; }
    public string? summary { get; set; }
    public DateTime snapshotDate { get; set; }
#pragma warning restore IDE1006 // Naming Styles
}