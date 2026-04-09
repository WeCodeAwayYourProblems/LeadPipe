using System.Text.Json.Serialization;

namespace LeadPipe.Infrastructure.Dto;

public class TokenDto
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = null!;
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = null!;
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    public string? Provider { get; set; }
}
