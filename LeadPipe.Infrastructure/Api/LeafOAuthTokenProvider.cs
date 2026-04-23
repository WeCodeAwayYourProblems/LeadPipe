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

internal sealed class LeafOAuthTokenProvider(
    ILeafSettings settings,
    ITokenCacheService cache,
    IOAuthTokenRepository tokenRepository,
    IHttpClientFactory httpClientFactory,
    IClock clock,
    ITranslate<TokenDto, OAuthTokenEntity> translate,
    ILogger<LeafOAuthTokenProvider> logger,
    string providerName
) : OAuthTokenProvider<LeafOAuthTokenProvider>(cache, tokenRepository, httpClientFactory, clock, translate, logger, providerName)
{
    readonly ILeafSettings _settings = settings;
    protected override Uri AuthorizationUri => new(_settings.LeafAuthorizationUrl!);

    protected override string OAuthClientName => _settings.LeafOAuthName!;

    protected override Task<Result<FormUrlEncodedContent>> Content(CancellationToken _)
    {
        if (_settings.LeafU is null)
            return Task.FromResult(Result.Failure<FormUrlEncodedContent>($"{nameof(_settings.LeafU)} cannot be null"));
        if (_settings.LeafP is null)
            return Task.FromResult(Result.Failure<FormUrlEncodedContent>($"{nameof(_settings.LeafP)} cannot be null"));

        Dictionary<string, string> rawContent = new()
        {
            { "username", _settings.LeafU! },
            { "password", _settings.LeafP! },
        };
        FormUrlEncodedContent content = new(rawContent);
        
        return Task.FromResult(Result.Success(content));
    }
}