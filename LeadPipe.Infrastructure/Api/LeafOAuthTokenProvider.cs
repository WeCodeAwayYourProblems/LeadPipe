using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LeadPipe.Infrastructure.Api;

internal sealed class LeafOAuthTokenProvider(
    ILeafSettings settings,
    IHttpClientFactory httpClientFactory,
    ILogger<LeafOAuthTokenProvider> logger,
    IClock clock,
    ITranslate<TokenDto, OAuthTokenEntity> translate,
    IOAuthTokenRepository tokenPersistence,
    string providerName
) : OAuthTokenProvider(tokenPersistence, clock, providerName)
{
    readonly ILeafSettings _settings = settings;
    readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    readonly ILogger<LeafOAuthTokenProvider> _logger = logger;
    readonly IClock _clock = clock;
    readonly ITranslate<TokenDto, OAuthTokenEntity> _translate = translate;
    readonly IOAuthTokenRepository _tokenPersistence = tokenPersistence;
    readonly string _providerName = providerName;
    const int _errorLimit = 5;
    public override async Task<Result<string>> ForceRefreshAsync(CancellationToken ct)
    {
        HttpClient client = _httpClientFactory.CreateClient(_settings.LeafOAuthName!);

        // Post and check response
        for (var attempt = 0; attempt <= _errorLimit; attempt++)
        {
            try
            {
                ct.ThrowIfCancellationRequested();

                // Build UrlEncoded
                Dictionary<string, string> rawContent = new()
                {
                    { "username", _settings.LeafU! },
                    { "password", _settings.LeafP! },
                };
                FormUrlEncodedContent content = new(rawContent);

                HttpResponseMessage response = await client.PostAsync(_settings.LeafAuthorizationUrl!, content, ct);
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
                tokenDto.Provider = _providerName;
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

}