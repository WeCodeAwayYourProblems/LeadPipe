namespace LeadPipe.Infrastructure.MySql.Settings;

public interface IMySqlSettings
{
    public string? MySqlConnectionString { get; set; }
    public string? Schema1 { get; set; }
    public string? Schema2 { get; set; }
}
