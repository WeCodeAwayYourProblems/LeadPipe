using CSharpFunctionalExtensions;

namespace LeadPipe.Infrastructure.Interfaces.Api;

internal interface IOAuthTokenProvider
{
    Task<Result<string>> GetValidAccessTokenAsync(CancellationToken ct);
    Task<Result<string>> ForceRefreshAsync(CancellationToken ct);
}

