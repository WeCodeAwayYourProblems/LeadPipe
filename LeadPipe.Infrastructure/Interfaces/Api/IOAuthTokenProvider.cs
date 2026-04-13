using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Api;

namespace LeadPipe.Infrastructure.Interfaces.Api;

internal interface IOAuthTokenProvider
{
    Task<Result<AccessToken>> ForceRefreshAsync(CancellationToken ct);
    Task<Result<string>> GetValidAccessTokenAsync(CancellationToken ct);
}

