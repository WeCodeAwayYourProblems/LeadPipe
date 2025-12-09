using LeadPipe.Domain;
using LeadPipe.Infrastructure.MySql.Settings;
using LeadPipe.Infrastructure.Settings;

namespace LeadPipe.Cli;

internal class Settings : IInfrastructureSettings, IDomainSettings, IMySqlSettings
{
    // IDwhSettings
    public string? SqlConnectionString1 { get; set; }
    public string? SqlConnectionString2 { get; set; }


    // Lab
    public string? LabId { get; set; }
    public string? LabSecret { get; set; }
    public string? LabName { get; set; }
    public string? LabUri { get; set; }
    public string? LabPlumbing { get; set; }
    public string? LabAuth { get; set; }
    public int LabConcurrentMax { get; set; }
    public string? LabToken { get; set; }
    public string? LabAccept { get; set; }

    // Infrastructure
    public string? LeafName { get; set; }
    public string? LeafTokenType { get; set; }
    public string? LeafRefreshToken { get; set; }
    public string? LeafBase { get; set; }
    public string? LeafAcctUuid { get; set; }
    public string? LeafUuid { get; set; }
    public string? LeafThreadsEndpoint { get; set; }
    public string? LeafMessagesEndpoint { get; set; }
    public int LeafConcurrentMax { get; set; }
    public string? PlumbingContext { get; set; }
    public string? MySqlConnectionString { get; set; }
    public string? Schema1 { get; set; }
    public string? Schema2 { get; set; }
    public string? CalliLocation { get; set; }
    public string? LeasedLocation { get; set; }
    public string? LibacionLocation { get; set; }
    public string? PanLocation { get; set; }
}
