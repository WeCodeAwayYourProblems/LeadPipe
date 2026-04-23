using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Repository;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class OAuthTokenRepository(PlumbingContext context) : IOAuthTokenRepository
{
    private readonly PlumbingContext _context = context;
    private readonly DbSet<OAuthTokenEntity> _set = context.Set<OAuthTokenEntity>();
    private static readonly string EntityName = nameof(OAuthTokenEntity);
    public async Task<Result<OAuthTokenEntity>> GetByProviderAsync(string providerName, CancellationToken ct = default)
    {
        try
        {
            var entity = await _set
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Provider == providerName, ct);

            if (entity is null)
                return Result.Failure<OAuthTokenEntity>($"{EntityName} not found for {providerName}");

            return Result.Success(entity);
        }
        catch (Exception ex) { return Result.Failure<OAuthTokenEntity>($"Failed to retrieve {EntityName} for {providerName}. Exception: {ex}"); }
    }

    public async Task<Result<OAuthTokenEntity>> UpsertAsync(OAuthTokenEntity token, CancellationToken ct = default)
    {
        try
        {
            // Check whether the input entity exists in the database
            OAuthTokenEntity? existing = await _set
                .FirstOrDefaultAsync(x => x.Provider == token.Provider, ct);

            if (existing is null)
                await _set.AddAsync(token, ct);
            else
            {
                if (existing.UnixUpdatedAtUtc >= token.UnixUpdatedAtUtc)
                    return Result.Success(existing);

                existing.AccessToken = token.AccessToken;
                existing.TokenType = token.TokenType;
                existing.RefreshToken = token.RefreshToken;
                existing.ExpiresAtUtc = token.ExpiresAtUtc;
                existing.UnixExpiresAtUtc = token.UnixExpiresAtUtc;
                existing.UpdatedAtUtc = token.UpdatedAtUtc;
                existing.UnixUpdatedAtUtc = token.UnixUpdatedAtUtc;
            }
            await _context.SaveChangesAsync(ct);
            return Result.Success(existing ?? token);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure<OAuthTokenEntity>(
                $"Concurrency conflict updating {EntityName} for {token.Provider}");
        }
        catch (DbUpdateException)
        {
            // Likely duplicate insert — retry as update
            var existing = await _set
                .FirstOrDefaultAsync(x => x.Provider == token.Provider, ct);

            if (existing is null)
                throw; // something else went wrong

            if (existing.UnixUpdatedAtUtc >= token.UnixUpdatedAtUtc)
                return Result.Success(existing);

            existing.AccessToken = token.AccessToken;
            existing.TokenType = token.TokenType;
            existing.RefreshToken = token.RefreshToken;
            existing.ExpiresAtUtc = token.ExpiresAtUtc;
            existing.UnixExpiresAtUtc = token.UnixExpiresAtUtc;
            existing.UpdatedAtUtc = token.UpdatedAtUtc;
            existing.UnixUpdatedAtUtc = token.UnixUpdatedAtUtc;

            await _context.SaveChangesAsync(ct);
            
            return Result.Success(existing);
        }
        catch (Exception ex) { return Result.Failure<OAuthTokenEntity>($"Failed to upsert {EntityName}. Value: {token}. Exception: {ex}"); }
    }
}