using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Infrastructure.Entity.Sqlite;

public sealed class OAuthTokenEntity
{
    public string? Provider { get; set; }
    public string AccessToken { get; set; } = null!;
    public string TokenType { get; set; } = null!;
    public string? RefreshToken { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public long UnixExpiresAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }    
    public long UnixUpdatedAtUtc { get; set; }
}