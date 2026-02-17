using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public abstract class PlumbingContextRepository<TEntity, TRepo>
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

    internal async Task<Result<List<T>>> LinkUpsertAsync<T>(
        List<T> links,
        CancellationToken ct) where T : class
    {
        // Mapping check
        var entityType = _context.Model.FindEntityType(typeof(T))
            ?? throw new InvalidOperationException($"{typeof(T).Name} is not mapped");

        var tableName = entityType.GetTableName();

        // Get the composite unique index (the one marked .IsUnique() in OnModelCreating)
        var businessKey = entityType.GetIndexes().First(i => i.IsUnique).Properties;

        // Column names from ef mapping
        var storeId = StoreObjectIdentifier.Table(tableName!, null);
        var id1Col = businessKey[0].GetColumnName(storeId);
        var id2Col = businessKey[1].GetColumnName(storeId);
        var phoneCol = entityType.FindProperty(nameof(CornCaliperLink.MatchingPhone))?.GetColumnName(storeId)
            ?? nameof(CornCaliperLink.MatchingPhone); // Standard across links
        var dateCol = entityType.FindProperty(nameof(CornCaliperLink.UnixMatchDate))?.GetColumnName(storeId)
            ?? nameof(CornCaliperLink.UnixMatchDate); // Standard across links

        // Use reflection to be reactive to property names
        var id1Prop = typeof(T).GetProperty(businessKey[0].Name);
        var id2Prop = typeof(T).GetProperty(businessKey[1].Name);
        var dateProp = typeof(T).GetProperty(dateCol);
        var phoneProp = typeof(T).GetProperty(phoneCol);

        // Compile once
        var id1Getter = CreateGetter(typeof(T).GetProperty(businessKey[0].Name)!);
        var id2Getter = CreateGetter(typeof(T).GetProperty(businessKey[1].Name)!);
        var phoneGetter = CreateGetter(typeof(T).GetProperty(phoneCol)!);
        var dateGetter = CreateGetter(typeof(T).GetProperty(dateCol)!);

        string tempTable = $"temp_{tableName}";

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            // Dynamic Temp Table
            string createTemp = $"""
                CREATE TEMP TABLE IF NOT EXISTS {tempTable} (
                    id1 INTEGER, id2 INTEGER, phone INTEGER, matchDate INTEGER
                );
                DELETE FROM {tempTable};
            """;
            await _context.Database.ExecuteSqlRawAsync(createTemp, ct);

            // Parameterized batching
            // SQLite default paramter limit is 999 for version < 3.32.0.
            // 4 params per row * 240 rows = 960 params.
            int index = 0;
            int batchSize = 240; 
            while (index < links.Count)
            {
                int take = Math.Min(batchSize, links.Count - index);
                var batch = links.GetRange(index, take);

                var values = new List<object>();
                var rows = new List<string>();
                for (int i = 0; i < batch.Count; i++)
                {
                    int o = i * 4;
                    rows.Add($"({{{o}}}, {{{o + 1}}}, {{{o + 2}}}, {{{o + 3}}})");
                    values.Add(id1Getter(batch[i]));
                    values.Add(id2Getter(batch[i]));
                    values.Add(phoneGetter(batch[i]));
                    values.Add(dateGetter(batch[i]));
                }

                string joined = $"INSERT INTO {tempTable} VALUES {string.Join(",", rows)}";
                await _context.Database.ExecuteSqlRawAsync(joined, values, ct);
                index += take;
            }

            // Dynamic UPSERT
            // Finds MIN(matchDate) per unique pair directly in SQLite
           string upsertSql = $"""
                INSERT INTO {tableName} ({id1Col}, {id2Col}, {phoneCol}, {dateCol})
                SELECT t.id1, t.id2, t.phone, MIN(t.matchDate) 
                FROM {tempTable} t
                WHERE t.phone <> 0
                GROUP BY t.id1, t.id2, t.phone
                ON CONFLICT({id1Col}, {id2Col}) DO UPDATE SET
                    {phoneCol} = excluded.{phoneCol},
                    {dateCol} = MIN({tableName}.{dateCol}, excluded.{dateCol});
            """;
            await _context.Database.ExecuteSqlRawAsync(upsertSql, ct);

            await transaction.CommitAsync(ct);
            return Result.Success(links);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Generic Upsert failed for {Table}", tableName);
            return Result.Failure<List<T>>(ex.ToString());
        }

        static Func<T, object> CreateGetter(PropertyInfo prop)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T));
            MemberExpression property = Expression.Property(parameter, prop);
            UnaryExpression cast = Expression.Convert(property, typeof(object));
            var result = Expression.Lambda<Func<T, object>>(cast, parameter).Compile();
            return result;
        }

    }

}
