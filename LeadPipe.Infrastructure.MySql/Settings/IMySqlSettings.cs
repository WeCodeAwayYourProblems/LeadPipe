namespace LeadPipe.Infrastructure.MySql.Settings;

public interface IMySqlSettings
{
    string? Schema1ConnectionString { get; set; }
    string? Schema2ConnectionString { get; set; }
    string? Schema3ConnectionString { get; set; }
    string? Schema1 { get; set; }
    string? Schema2 { get; set; }
    string? Schema3 { get; set; }
    string? CornTableName { get; set; }
    string? SandTableName { get; set; }
    string? CustardTableName { get; set; }
}
