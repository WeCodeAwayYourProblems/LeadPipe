using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Api;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Infrastructure.Settings;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Api;

internal sealed class YellerOAuthTokenProvider(
    IYellerSettings settings,
    ITokenCacheService cache,
    IOAuthTokenRepository tokenRepository,
    IHttpClientFactory httpClientFactory,
    IClock clock,
    ITranslate<TokenDto, OAuthTokenEntity> translate,
    ILogger<YellerOAuthTokenProvider> logger,
    string providerName
    ) : OAuthTokenProvider<YellerOAuthTokenProvider>(cache, tokenRepository, httpClientFactory, clock, translate, logger, providerName)
{
    readonly IYellerSettings _settings = settings;

    protected override Uri AuthorizationUri => new(_settings.YellerAuthUrl!);

    protected override string OAuthClientName => _settings.YellerOAuthName!;

    protected override long BufferSeconds => 172_800;

    protected override async Task<Result<FormUrlEncodedContent>> Content(CancellationToken ct)
    {
        Result<OAuthTokenEntity> token = await _tokenPersistence.GetByProviderAsync(_providerName, ct);

        // Check token return
        if (token.IsFailure)
        {
            _logger.LogError("Provider={Provider}. Failed to retrieve existing refresh token", _providerName);
            return Result.Failure<FormUrlEncodedContent>("Failed to retrieve existing refresh token");
        }
        if (token.Value.RefreshToken is null)
        {
            _logger.LogError("{Persistence} failed to persist refresh token", nameof(IOAuthTokenRepository));
            return Result.Failure<FormUrlEncodedContent>("Failed to retrieve existing refresh token from persistence");
        }

        // Build FormUrlEncodedContent
        Dictionary<string, string> rawContent = new()
        {
            { "grant_type", "refresh_token" },
            { "client_id", _settings.YellerId! },
            { "client_secret", _settings.YellerSecret! },
            { "refresh_token", token.Value.RefreshToken! }
        };

        FormUrlEncodedContent content = new(rawContent);

        return content;
    }
}
