namespace LeadPipe.Infrastructure.SettingsInterfaces;

public interface ILeafSettings
{
    public string? LeafName { get; set; }
    public string? LeafTokenType { get; set; }
    public string? LeafRefreshToken { get; set; }
    public string? LeafBase { get; set; }
    public string? LeafAcctUuid { get; set; }
    public string? LeafUuid { get; set; }
    public string? LeafThreadsEndpoint { get; set; }
    public string? LeafMessagesEndpoint { get; set; }
}
