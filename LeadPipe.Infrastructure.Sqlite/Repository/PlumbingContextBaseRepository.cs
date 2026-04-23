using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Linq.Expressions;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public abstract class PlumbingContextBaseRepository<TEntity, TRepo>
    (
        PlumbingContext context,
        ILogger<TRepo> logger
    ) : IRepository<TEntity>
    where TEntity : class, IEntity
{
    protected readonly PlumbingContext _context = context;
    protected readonly DbSet<TEntity> _set = context.Set<TEntity>();
    protected readonly ILogger<TRepo> _logger = logger;

    #region Protected
    protected static string Clean(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value
            .Replace("\0", string.Empty)
            .Replace("'", "''");
    }
    protected static void AssertNotString<T>(string propertyName)
    {
        var type = typeof(T).GetProperty(propertyName)!.PropertyType;

        if (type == typeof(string))
            throw new InvalidOperationException(
                $"{typeof(T).Name}.{propertyName} must not be string when used in raw SQL upsert.");
    }
    protected static string IsoString { get; } = "yyyy-MM-dd HH:mm:ss";
    #endregion

    public async Task<Result<List<TEntity>>> GetAllWithDetailsAsync(CancellationToken ct = default)
    {
        try
        {
            var list = await WithIncludes(_set.AsNoTracking())
                .ToListAsync(ct);

            return Result.Success(list);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<TEntity>>(ex.ToString());
        }
    }

    public async Task<Result<List<TEntity>>> FindWithDetailsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
    {
        try
        {
            var list = await WithIncludes(_set.AsNoTracking())
                .Where(predicate)
                .ToListAsync(ct);

            return Result.Success(list);
        }
        catch (Exception ex) { return Result.Failure<List<TEntity>>(ex.ToString()); }
    }

    #region Virtual
    public virtual async Task<Result<List<TEntity>>> GetAllAsync(
        CancellationToken ct = default)
    {
        try
        {
            List<TEntity> result = await _set.AsNoTracking().ToListAsync(ct);
            return Result.Success(result);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<TEntity>>(ex.ToString());
        }
    }

    public virtual async Task<Result<List<TEntity>>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default)
    {
        try
        {
            var list = await _set
                .AsNoTracking()
                .Where(predicate)
                .ToListAsync(ct);

            return Result.Success(list);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<TEntity>>(ex.ToString());
        }
    }
    #endregion

    #region Abstract
    protected abstract IQueryable<TEntity> WithIncludes(IQueryable<TEntity> q);

    public abstract Task<Result<List<TEntity>>> UpsertRangeAsync(
        List<TEntity> entities,
        CancellationToken ct = default);
    #endregion

}
