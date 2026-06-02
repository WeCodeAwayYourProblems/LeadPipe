using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SQLitePCL;
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
    protected static string IsoString { get; } = "yyyy-MM-dd HH:mm:ss";
    protected int? _parameterLimit;
    protected int ParameterLimit => _parameterLimit ??= GetParameterLimit();
    protected int GetParameterLimit()
    {
        if (_context.Database.GetDbConnection() is not SqliteConnection conn)
            return 999; // Historical Paramter limit

        if (conn.State != System.Data.ConnectionState.Open)
            conn.Open();

        // Passing -1 queries the current limit without changing it
        return raw.sqlite3_limit(conn.Handle, raw.SQLITE_LIMIT_VARIABLE_NUMBER, -1);
    }
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
