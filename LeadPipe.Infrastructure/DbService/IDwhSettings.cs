namespace LeadPipe.Infrastructure.DbService;

public interface IDwhSettings
{
    public string? SqlConnectionString1 { get; set; }
    public string? SqlConnectionString2 { get; set; }
}
