using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Api;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LeadPipe.Infrastructure.Api;

internal abstract class OAuthTokenProvider<T>(
    ITokenCacheService cache,
    IOAuthTokenRepository tokenRepository,
    IHttpClientFactory httpClientFactory,
    IClock clock,
    ITranslate<TokenDto, OAuthTokenEntity> translate,
    ILogger<T> logger,
    string providerName) : IOAuthTokenProvider
{
    readonly ITokenCacheService _cache = cache;
    protected readonly IOAuthTokenRepository _tokenPersistence = tokenRepository;
    readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    readonly IClock _clock = clock;
    readonly ITranslate<TokenDto, OAuthTokenEntity> _translate = translate;
    protected readonly ILogger<T> _logger = logger;
    protected readonly string _providerName = providerName;
    protected abstract Task<Result<FormUrlEncodedContent>> Content(CancellationToken ct);
    protected abstract Uri AuthorizationUri { get; }
    protected abstract string OAuthClientName { get; }
    protected virtual int ErrorLimit { get; } = 5;
    protected virtual long BufferSeconds { get; } = 60;

    public async Task<Result<AccessToken>> ForceRefreshAsync(CancellationToken ct)
    {
        HttpClient client = _httpClientFactory.CreateClient(OAuthClientName);

        // Post and check response
        for (var attempt = 0; attempt < ErrorLimit; attempt++)
        {
            try
            {
                ct.ThrowIfCancellationRequested();
                
                var contentResult = await Content(ct);
                if (contentResult.IsFailure)
                    return Result.Failure<AccessToken>(contentResult.Error);
                using HttpResponseMessage response = await client.PostAsync(AuthorizationUri, contentResult.Value, ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Provider={Provider}. Status Code={StatusCode}. Reason Phrase={ReasonPhrase}. Total Errors={Errors}",
                        _providerName,
                        response.StatusCode,
                        response.ReasonPhrase,
                        attempt + 1);
                    if (attempt == ErrorLimit - 1)
                        return Result.Failure<AccessToken>(response.ReasonPhrase);
                    continue;
                }

                var commitResult = await CommitTokenAsync(response, ct);
                if (commitResult.IsFailure)
                    return Result.Failure<AccessToken>(commitResult.Error);

                return commitResult;
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Execution error. Provider={Provider}", _providerName);
                if (attempt == ErrorLimit - 1)
                    return Result.Failure<AccessToken>(ex.Message);
                continue;
            }
        }

        _logger.LogError("Provider failed to provide token after several attempts. Provider={Provider}. Attempts={Attempts}",
            _providerName,
            ErrorLimit);
        return Result.Failure<AccessToken>($"{_providerName} failed to fetch token");
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
        var inMemory = await _cache.GetOrAddAsync(
            key: $"oauth::{_providerName}",
            factory: GetValidAccessTokenInternal,
            bufferSeconds: BufferSeconds,
            ct: ct);
        return inMemory;
    }

    private async Task<Result<AccessToken>> GetValidAccessTokenInternal(CancellationToken ct)
    {
        // Retrieve existing token
        var entity = await _tokenPersistence.GetByProviderAsync(_providerName, ct);
        if (entity.IsFailure)
            return await ForceRefreshAsync(ct);

        // If token is expired, refresh it
        var now = _clock.UtcNow.ToUnixTimeSeconds();
        if (entity.Value.UnixExpiresAtUtc <= now + BufferSeconds)
            return await ForceRefreshAsync(ct);

        return FromEntity(entity.Value);
    }

    protected static AccessToken FromEntity(OAuthTokenEntity e) => new(e.AccessToken, e.UnixExpiresAtUtc);
    private async Task<Result<AccessToken>> CommitTokenAsync(
        HttpResponseMessage response,
        CancellationToken originalCt)
    {
        // Create bounded commit token
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(originalCt);
        cts.CancelAfter(TimeSpan.FromSeconds(10)); // tune this
        var ct = cts.Token;

        try
        {
            string responseString = await response.Content.ReadAsStringAsync(ct);

            TokenDto? tokenDto = JsonSerializer.Deserialize<TokenDto>(responseString);
            if (tokenDto is null)
            {
                _logger.LogError("Deserialization failed during commit phase. Provider={Provider}", _providerName);
                return Result.Failure<AccessToken>("Failed to deserialize token");
            }

            tokenDto.Provider = _providerName;
            OAuthTokenEntity entity = _translate.Translate(tokenDto);

            // Retry persistence
            var persisted = await PersistWithRetryAsync(entity, ct);
            if (persisted.IsFailure)
                return Result.Failure<AccessToken>(persisted.Error);

            return FromEntity(persisted.Value);
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("Commit phase timed out. Provider={Provider}", _providerName);
            return Result.Failure<AccessToken>("Commit phase timed out");
        }
    }
    private async Task<Result<OAuthTokenEntity>> PersistWithRetryAsync(
        OAuthTokenEntity entity,
        CancellationToken ct)
    {
        const int maxAttempts = 3;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var result = await _tokenPersistence.UpsertAsync(entity, ct);

                if (result.IsSuccess)
                    return result;

                _logger.LogWarning(
                    "Token persistence failed (attempt {Attempt}/{Max}). Provider={Provider}. Error={Error}",
                    attempt,
                    maxAttempts,
                    _providerName,
                    result.Error);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Exception during token persistence (attempt {Attempt}/{Max}). Provider={Provider}",
                    attempt,
                    maxAttempts,
                    _providerName);
            }

            // small backoff
            await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), ct);
        }

        _logger.LogError("Failed to persist token after retries. Provider={Provider}", _providerName);

        return Result.Failure<OAuthTokenEntity>("Failed to persist token after retries");
    }
}
