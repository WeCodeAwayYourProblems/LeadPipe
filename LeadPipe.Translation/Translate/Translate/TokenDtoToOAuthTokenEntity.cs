using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.Translate;

internal class TokenDtoToOAuthTokenEntity(IClock clock) : ITranslate<TokenDto, OAuthTokenEntity>
{
    private readonly IClock _clock = clock;
    public OAuthTokenEntity Translate(TokenDto t)
    {
        var now = _clock.UtcNow;
        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(now.ToUnixTimeSeconds() + t.ExpiresIn);
        var result = new OAuthTokenEntity
        {
            AccessToken = t.AccessToken,
            TokenType = t.TokenType,
            RefreshToken = t.RefreshToken,
            ExpiresAtUtc = expiresAt,
            UnixExpiresAtUtc = expiresAt.ToUnixTimeSeconds(),
            UpdatedAtUtc = now,
            UnixUpdatedAtUtc = now.ToUnixTimeSeconds()
        };
        return result;
    }
}
