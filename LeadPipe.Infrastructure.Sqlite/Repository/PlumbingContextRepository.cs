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

    internal async Task<Result<List<T>>> UpsertLinkRangeAsync<T>(
        List<T> links,
        CancellationToken ct) where T : class
    {
        // Empty check
        if (links.Count == 0)
            return Result.Success(links);

        // Mapping check
        IEntityType entityType = _context.Model.FindEntityType(typeof(T))
            ?? throw new InvalidOperationException($"{typeof(T).Name} is not mapped");

        var tableName = entityType.GetTableName();

        // Get the composite unique index (the one marked .IsUnique() in OnModelCreating)
        var businessKey = entityType.GetIndexes().First(i => i.IsUnique).Properties;
        var id1Key = businessKey[0];
        var id2Key = businessKey[1];

        // Column names from ef mapping
        var storeId = StoreObjectIdentifier.Table(tableName!, null);
        var id1Col = id1Key.GetColumnName(storeId);
        var id2Col = id2Key.GetColumnName(storeId);
        var phoneCol = entityType.FindProperty(nameof(CornCaliperLink.MatchingPhone))?.GetColumnName(storeId)
            ?? nameof(CornCaliperLink.MatchingPhone); // Standard across links
        var dateCol = entityType.FindProperty(nameof(CornCaliperLink.UnixMatchDate))?.GetColumnName(storeId)
            ?? nameof(CornCaliperLink.UnixMatchDate); // Standard across links

        // Get parent table names 
        var parentTable1 = GetParentTable(entityType, id1Key);
        var parentTable2 = GetParentTable(entityType, id2Key);
        var parentTable1Name = parentTable1.Name;
        var parentTable2Name = parentTable2.Name;
        var parentTable1NameIdentifier = StoreObjectIdentifier.Table(parentTable1Name, null);
        var parentTable2NameIdentifier = StoreObjectIdentifier.Table(parentTable2Name, null);
        var parentTable1KeyCol = parentTable1
            .FindPrimaryKey()!.Properties[0]
            .GetColumnName(parentTable1NameIdentifier);
        var parentTable2KeyCol = parentTable2
            .FindPrimaryKey()!.Properties[0]
            .GetColumnName(parentTable2NameIdentifier);

        // Compile once
        var id1Getter = CreateGetter(typeof(T).GetProperty(id1Key.Name)!);
        var id2Getter = CreateGetter(typeof(T).GetProperty(id2Key.Name)!);
        var phoneGetter = CreateGetter(typeof(T).GetProperty(phoneCol)!);
        var dateGetter = CreateGetter(typeof(T).GetProperty(dateCol)!);

        string tempTable = $"temp_{tableName}";

        // Debug info
        _logger.LogDebug(
        "Resolved parent table keys: {Parent1}.{Key1}, {Parent2}.{Key2}",
            parentTable1Name, parentTable1KeyCol, parentTable2Name, parentTable2KeyCol
        );

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            // Dynamic Temp Table
            string createTemp = $"""
                DROP TABLE IF EXISTS {tempTable};                
                CREATE TEMP TABLE {tempTable} (
                    id1 INTEGER, 
                    id2 INTEGER, 
                    phone INTEGER, 
                    matchDate INTEGER
                );
            """;
            await _context.Database.ExecuteSqlRawAsync(createTemp, ct);

            // Parameterized batching
            // SQLite default paramter limit is 999 for version < 3.32.0.
            // 4 params per row * 240 rows = 960 params.
            int batchSize = 240;
            for (int i = 0; i < links.Count; i += batchSize)
            {
                var batch = links.GetRange(i, Math.Min(batchSize, links.Count - i));

                var values = new List<object>();
                var rows = new List<string>();
                for (int j = 0; j < batch.Count; j++)
                {
                    int o = j * 4;
                    rows.Add($"({{{o}}}, {{{o + 1}}}, {{{o + 2}}}, {{{o + 3}}})");
                    values.Add(id1Getter(batch[j]));
                    values.Add(id2Getter(batch[j]));
                    values.Add(phoneGetter(batch[j]));
                    values.Add(dateGetter(batch[j]));
                }

                string joined = $"INSERT INTO {tempTable} VALUES {string.Join(",", rows)}";
                await _context.Database.ExecuteSqlRawAsync(joined, values, ct);
            }

            // Keep in mind that deduplication here is exactly the way we want.
            // Deduplication in this way is much faster here than in memory, especially for hundreds of thousands of upserts
            string updateSql = $"""
                UPDATE {tableName}
                SET {phoneCol} = (
                        SELECT t.phone
                        FROM {tempTable} t
                        WHERE t.id1 = {tableName}.{id1Col}
                          AND t.id2 = {tableName}.{id2Col}
                          AND t.phone <> 0
                        ORDER BY t.matchDate ASC
                        LIMIT 1
                    ),
                    {dateCol} = (
                        SELECT t.matchDate
                        FROM {tempTable} t
                        WHERE t.id1 = {tableName}.{id1Col}
                          AND t.id2 = {tableName}.{id2Col}
                          AND t.phone <> 0
                        ORDER BY t.matchDate ASC
                        LIMIT 1
                    )
                WHERE EXISTS (
                    SELECT 1
                    FROM {tempTable} temp
                    WHERE temp.id1 = {tableName}.{id1Col}
                      AND temp.id2 = {tableName}.{id2Col}
                      AND temp.phone <> 0
                );
            """;
            int totalUpdated = await _context.Database.ExecuteSqlRawAsync(updateSql, ct);


            string insertSql = $"""
                INSERT INTO {tableName} ({id1Col}, {id2Col}, {phoneCol}, {dateCol})
                SELECT temp.id1, temp.id2, temp.phone, MIN(temp.matchDate)
                FROM {tempTable} temp
                WHERE temp.phone <> 0
                  AND EXISTS (SELECT 1 FROM {parentTable1Name} p1 WHERE p1.{parentTable1KeyCol} = temp.id1)
                  AND EXISTS (SELECT 1 FROM {parentTable2Name} p2 WHERE p2.{parentTable2KeyCol} = temp.id2)
                  AND NOT EXISTS (
                      SELECT 1 FROM {tableName} t
                      WHERE t.{id1Col} = temp.id1 AND t.{id2Col} = temp.id2
                  )
                GROUP BY temp.id1, temp.id2;
            """;
            int totalInserted = await _context.Database.ExecuteSqlRawAsync(insertSql, ct);

            _logger.LogInformation(
                "{Entity} upsert complete: Incoming={Incoming}, Updated={Updated}, Inserted={Inserted}",
                typeof(T).Name,
                links.Count,
                totalUpdated,
                totalInserted);

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

        static IEntityType GetParentTable(IEntityType entityType, IProperty fkProp)
        {
            // Find the foreign key associated with this property
            var fk = entityType.GetForeignKeys()
                .FirstOrDefault(f => f.Properties.Contains(fkProp))
                ?? throw new InvalidOperationException($"no foreign key found for property {fkProp.Name} on {entityType.Name}");

            // the principal (parent) entity type
            IEntityType principal = fk.PrincipalEntityType;

            return principal;
        }
    }

}
