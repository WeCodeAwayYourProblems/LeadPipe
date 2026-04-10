using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Interfaces.Api;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

namespace LeadPipe.Infrastructure.Api;

internal abstract class OAuthTokenProvider(
    IOAuthTokenRepository tokenRepository,
    IClock clock,
    string providerName) : IOAuthTokenProvider
{
    readonly IOAuthTokenRepository _tokenPersistence = tokenRepository;
    readonly IClock _clock = clock;
    readonly string _providerName = providerName;
    public abstract Task<Result<string>> ForceRefreshAsync(CancellationToken ct);

    /// <summary>
    /// Checks persistence for existing token. If the token is not expired, it returns the existing token. If the token is expired or about to expire, calls api for a new token, persists the token, and returns the new token.
    /// </summary>
    /// <param name="_providerName"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<Result<string>> GetValidAccessTokenAsync(CancellationToken ct)
    {
        // Retrieve existing token
        var entity = await _tokenPersistence.GetByProviderAsync(_providerName, ct);
        if (entity.IsFailure)
            return await ForceRefreshAsync(ct);

        // If token is expired, refresh it
        var now = _clock.UtcNow.ToUnixTimeSeconds();
        long bufferSeconds = 60;
        if (entity.Value.UnixExpiresAtUtc <= now + bufferSeconds)
            return await ForceRefreshAsync(ct);

        return entity.Value.AccessToken;
    }
}
