using LeadPipe.Infrastructure.Dto;

namespace LeadPipe.Infrastructure.Settings;

public interface ILeafSettings
{
    string? LeafOAuthName { get; set; }
    string? LeafAuthorizationUrl { get; set; }
    string? LeafName { get; set; }
    string? LeafBase { get; set; }
    string? LeafAcctUuid { get; set; }
    string? LeafUuid { get; set; }
    string? LeafThreadsEndpoint { get; set; }
    string? LeafMessagesEndpoint { get; set; }
    int LeafConcurrentMax { get; set; }
    string? LeafU { get; set; }
    string? LeafP { get; set; }
}
