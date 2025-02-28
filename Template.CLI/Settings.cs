using Template.Infrastructure.SettingsInterfaces;

namespace Template.CLI;

internal class Settings : IInfrastructureSettings
{
    // IDwhSettings
    public string? SqlConnectionString1 { get; set; }
    public string? SqlConnectionString2 { get; set; }

    // Named Http Clients
    public string? Name1 { get; set; }
    public string? Name1Token { get; set; }
    public string? Name1AccountId { get; set; }
}