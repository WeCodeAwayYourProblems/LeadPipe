using CSharpFunctionalExtensions;
using LeadPipe.Core;
using LeadPipe.Infrastructure.Interfaces.Api;
using LeadPipe.Infrastructure.Interfaces.Core;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace LeadPipe.Infrastructure.Api;

internal sealed class TokenCacheService(
    IMemoryCache cache,
    IClock clock
) : ITokenCacheService
{
    private readonly IMemoryCache _cache = cache;
    private readonly IClock _clock = clock;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public async Task<Result<string>> GetOrAddAsync(string key, Func<CancellationToken, Task<Result<AccessToken>>> factory, long bufferSeconds, CancellationToken ct)
    {
        if (_cache.TryGetValue(key, out AccessToken cached))
        {
            var n1 = _clock.UtcNow.ToUnixTime();
            if (!cached.IsExpired(n1, bufferSeconds))
                return cached.Value;
        }

        var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync(ct);
        try
        {
            if (_cache.TryGetValue(key, out cached))
            {
                var n1 = _clock.UtcNow.ToUnixTime();
                if (!cached.IsExpired(n1, bufferSeconds))
                    return cached.Value;
            }

            Result<AccessToken> result = await factory(ct);
            if (result.IsFailure)
                return Result.Failure<string>(result.Error);

            var token = result.Value;
            var now = _clock.UtcNow.ToUnixTime();
            var ttl = token.GetTtlSeconds(now, bufferSeconds);

            if (ttl > 0)
                _cache.Set(key, token, TimeSpan.FromSeconds(ttl));

            return token.Value;
        }
        finally
        {
            semaphore.Release();

            if (semaphore.CurrentCount == 1)
                _locks.TryRemove(key, out _);
        }

    }
}
