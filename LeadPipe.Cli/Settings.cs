using LeadPipe.Domain;
using LeadPipe.Infrastructure.Settings;

namespace LeadPipe.Cli;

internal class Settings : IInfrastructureSettings, IDomainSettings
{
    // IDwhSettings
    public string? SqlConnectionString1 { get; set; }
    public string? SqlConnectionString2 { get; set; }

    // Named Http Clients
    public string? Name1 { get; set; }
    public string? Name1Token { get; set; }
    public string? Name1AccountId { get; set; }

    // Infrastructure
    public string? LeafName { get; set; }
    public string? LeafTokenType { get; set; }
    public string? LeafRefreshToken { get; set; }
    public string? LeafBase { get; set; }
    public string? LeafAcctUuid { get; set; }
    public string? LeafUuid { get; set; }
    public string? LeafThreadsEndpoint { get; set; }
    public string? LeafMessagesEndpoint { get; set; }
    public string? PlumbingContext { get; set; }
}