namespace LeadPipe.Infrastructure.MySql.Settings;

public interface IMySqlSettings
{
    public string? Schema2ConnectionString { get; set; }
    public string? SchemaConnectionString { get; set; }
    public string? Schema1 { get; set; }
    public string? Schema2 { get; set; }
}
