using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Infrastructure.Entity;

public sealed class OAuthTokenEntity
{
    public required string Provider { get; set; }
    public string AccessToken { get; set; } = null!;
    public string TokenType { get; set; } = null!;
    public string? RefreshToken { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public long UnixExpiresAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public long UnixUpdatedAtUtc { get; set; }    
    public override string ToString() => $"""
        {nameof(Provider)}: {Provider}.
        {nameof(AccessToken)}: {AccessToken}.
        {nameof(TokenType)}: {TokenType}.
        {nameof(RefreshToken)}: {RefreshToken}.
        {nameof(ExpiresAtUtc)}: {ExpiresAtUtc}.
        {nameof(UnixExpiresAtUtc)}: {UnixExpiresAtUtc}.
        {nameof(UpdatedAtUtc)}: {UpdatedAtUtc}.
        {nameof(UnixUpdatedAtUtc)}: {UnixUpdatedAtUtc}.
    """;
}