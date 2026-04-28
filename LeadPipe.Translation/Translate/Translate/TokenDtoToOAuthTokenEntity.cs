using LeadPipe.Core;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Translation.Translate.Translate;

internal class TokenDtoToOAuthTokenEntity(IClock clock) : ITranslate<TokenDto, OAuthTokenEntity>
{
    private readonly IClock _clock = clock;
    public OAuthTokenEntity Translate(TokenDto t)
    {
        if(t.Provider is null)
            throw new InvalidOperationException($"{nameof(t.Provider)} cannot be null");

        var now = _clock.UtcNow;
        var expiresAt = DateTimeOffsetExt.FromUnixTime(now.ToUnixTime() + t.ExpiresIn);
        var result = new OAuthTokenEntity
        {
            Provider = t.Provider,
            AccessToken = t.AccessToken,
            TokenType = t.TokenType,
            RefreshToken = t.RefreshToken,
            ExpiresAtUtc = expiresAt,
            UnixExpiresAtUtc = expiresAt.ToUnixTime(),
            UpdatedAtUtc = now,
            UnixUpdatedAtUtc = now.ToUnixTime()
        };
        return result;
    }
}
