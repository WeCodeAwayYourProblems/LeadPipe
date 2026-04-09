using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Api;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LeadPipe.Infrastructure.Api;

internal sealed class YellerOAuthTokenProvider(
    IYellerSettings settings,
    IOAuthTokenRepository repo,
    IClock clock,
    ITranslate<TokenDto, OAuthTokenEntity> translate,
    IHttpClientFactory httpClientFactory,
    ILogger<YellerOAuthTokenProvider> logger,
    string providerName
    ) : IOAuthTokenProvider
{
    private readonly IYellerSettings _settings = settings;
    private readonly IOAuthTokenRepository _tokenPersistence = repo;
    private readonly IClock _clock = clock;
    private readonly ITranslate<TokenDto, OAuthTokenEntity> _translate = translate;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<YellerOAuthTokenProvider> _logger = logger;
    private readonly string _providerName = providerName;
    const int _errorLimit = 5;

    /// <summary>
    /// Calls the api for a new token, persists the token, and then returns the token
    /// </summary>
    /// <param name="_providerName"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<Result<string>> ForceRefreshAsync(CancellationToken ct)
    {
        HttpClient client = _httpClientFactory.CreateClient(_settings.YellerOAuthName!);
        Result<OAuthTokenEntity> token = await _tokenPersistence.GetByProviderAsync(_providerName, ct);

        // Check token return
        if (token.IsFailure)
        {
            _logger.LogError("Provider={Provider}. Failed to retrieve existing refresh token", _providerName);
            return Result.Failure<string>("Failed to retrieve existing refresh token");
        }
        if (token.Value.RefreshToken is null)
        {
            _logger.LogError("{Persistence} failed to persist refresh token", nameof(IOAuthTokenRepository));
            return Result.Failure<string>("Failed to retrieve existing refresh token from persistence");
        }

        // Post and check response
        for (var attempt = 0; attempt < _errorLimit; attempt++)
        {
            try
            {
                ct.ThrowIfCancellationRequested();

                // Build FormUrlEncodedContent
                Dictionary<string, string> rawContent = new()
                {
                    { "grant_type", "refresh_token" },
                    { "client_id", _settings.YellerId! },
                    { "client_secret", _settings.YellerSecret! },
                    { "refresh_token", token.Value.RefreshToken! }
                };
                FormUrlEncodedContent content = new(rawContent);

                HttpResponseMessage response = await client.PostAsync(_settings.YellerAuthUrl, content, ct);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Provider={Provider}. Status Code={StatusCode}. Reason Phrase={ReasonPhrase}. Total Errors={Errors}",
                        _providerName,
                        response.StatusCode,
                        response.ReasonPhrase,
                        attempt + 1);
                    if (attempt == _errorLimit - 1)
                        return Result.Failure<string>(response.ReasonPhrase);
                    continue;
                }

                // Convert the response
                string responseString = await response.Content.ReadAsStringAsync(ct);
                TokenDto? tokenDto = JsonSerializer.Deserialize<TokenDto>(responseString);
                if (tokenDto is null)
                {
                    _logger.LogError("Deserialization Error. Provider={Provider}. Total Errors={Errors}. Response String={ResponseString}",
                        _providerName,
                        attempt + 1,
                        responseString
                        );
                    if (attempt == _errorLimit - 1)
                        return Result.Failure<string>($"{nameof(ForceRefreshAsync)}: Failed to deserialize token.");
                    continue;
                }

                // Translate the token
                OAuthTokenEntity e = _translate.Translate(tokenDto);
                e.Provider = _providerName;

                // Persist the token
                Result<OAuthTokenEntity> persisted = await _tokenPersistence.UpsertAsync(e, ct);
                if (persisted.IsFailure)
                {
                    _logger.LogError("Provider failed to persist the token. Provider={Provider}. Token={Token}. Total Errors={Errors}. Error Message={ErrorMessage}",
                        _providerName,
                        tokenDto,
                        attempt + 1,
                        persisted.Error);
                    if (attempt == _errorLimit - 1)
                        return Result.Failure<string>(persisted.Error);
                    continue;
                }

                return persisted.Value.AccessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Execution error. Provider={Provider}", _providerName);
                if (attempt == _errorLimit - 1)
                    return Result.Failure<string>(ex.Message);
                continue;
            }
        }

        _logger.LogError("Provider failed to provide token after several attempts. Provider={Provider}. Attempts={Attempts}",
            _providerName,
            _errorLimit);
        return Result.Failure<string>($"{_providerName} failed to fetch token");
    }

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
            return Result.Failure<string>(entity.Error);

        // If token is expired, refresh it
        var now = _clock.UtcNow.ToUnixTimeSeconds();
        long bufferSeconds = 60;
        if (entity.Value.UnixExpiresAtUtc <= now + bufferSeconds)
            return await ForceRefreshAsync(ct);

        return entity.Value.AccessToken;
    }
}
