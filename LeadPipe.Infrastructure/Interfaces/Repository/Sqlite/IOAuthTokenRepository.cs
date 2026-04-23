using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity;

namespace LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

public interface IOAuthTokenRepository
{
    Task<Result<OAuthTokenEntity>> GetByProviderAsync(string providerName, CancellationToken ct = default);
    Task<Result<OAuthTokenEntity>> UpsertAsync(OAuthTokenEntity token, CancellationToken ct = default);
}