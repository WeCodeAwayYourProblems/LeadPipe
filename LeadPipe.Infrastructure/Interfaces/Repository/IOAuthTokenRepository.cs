using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity;

namespace LeadPipe.Infrastructure.Interfaces.Repository;

public interface IOAuthTokenRepository
{
    Task<Result<OAuthTokenEntity>> GetByProviderAsync(string providerName, CancellationToken ct = default);
    Task<Result<OAuthTokenEntity>> UpsertAsync(OAuthTokenEntity token, CancellationToken ct = default);
}