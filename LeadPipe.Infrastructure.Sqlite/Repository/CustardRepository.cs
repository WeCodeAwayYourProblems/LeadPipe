using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Database.Configuration;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class CustardRepository
    (
        PlumbingContext context,
        ILogger<CustardRepository> logger
    ) : PlumbingContextEntityRepository<CustardEntity, CustardRepository>(context, logger), IRepository<CustardEntity>
{
    protected override IQueryable<CustardEntity> WithIncludes(IQueryable<CustardEntity> q)
    {
        return q
            .Include(q => q.SandEntities)
            .Include(q => q.CustardCaliperLinks)
            .Include(q => q.CustardCornLinks)
            .Include(q => q.CustardPlumbingLinks);
    }

    protected override UpsertFields EntityDetails { get; } =
    new(
        TableName: TableNames.CustardEntitiesName,
        TempTable: $"temp_{TableNames.CustardEntitiesName}",
        EntityName: nameof(CustardEntity),
        ColumnCount: 7);

    protected override string CreateTempTable => $"""
        CREATE TEMP TABLE IF NOT EXISTS {EntityDetails.TempTable} (
            {nameof(CustardEntity.Id)} INTEGER PRIMARY KEY,
            {nameof(CustardEntity.Active)} INTEGER,
            {nameof(CustardEntity.PhoneNumber)} INTEGER,
            {nameof(CustardEntity.PhoneNumber2)} INTEGER,
            {nameof(CustardEntity.Date)} TEXT,
            {nameof(CustardEntity.UnixDate)} INTEGER,
            {nameof(CustardEntity.UnixCancelDate)} INTEGER
        ) WITHOUT ROWID;
        DELETE FROM {EntityDetails.TempTable};
    """;

    protected override string UpdateSql => $"""
        UPDATE {TableNames.CustardEntitiesName} 
        SET 
            {nameof(CustardEntity.Active)} = temp.{nameof(CustardEntity.Active)},
            {nameof(CustardEntity.PhoneNumber)} = temp.{nameof(CustardEntity.PhoneNumber)},
            {nameof(CustardEntity.PhoneNumber2)} = temp.{nameof(CustardEntity.PhoneNumber2)},
            {nameof(CustardEntity.Date)} = temp.{nameof(CustardEntity.Date)},
            {nameof(CustardEntity.UnixDate)} = temp.{nameof(CustardEntity.UnixDate)},
            {nameof(CustardEntity.UnixCancelDate)} = temp.{nameof(CustardEntity.UnixCancelDate)}
        FROM {EntityDetails.TempTable} temp
        WHERE temp.{nameof(CustardEntity.Id)} = {TableNames.CustardEntitiesName}.{nameof(CustardEntity.Id)};
    """;

    protected override string InsertSql => $"""
        INSERT INTO {TableNames.CustardEntitiesName} (
            {nameof(CustardEntity.Id)},
            {nameof(CustardEntity.Active)}, 
            {nameof(CustardEntity.PhoneNumber)}, 
            {nameof(CustardEntity.PhoneNumber2)}, 
            {nameof(CustardEntity.Date)}, 
            {nameof(CustardEntity.UnixDate)}, 
            {nameof(CustardEntity.UnixCancelDate)}
        )
        SELECT 
            temp.{nameof(CustardEntity.Id)},
            temp.{nameof(CustardEntity.Active)}, 
            temp.{nameof(CustardEntity.PhoneNumber)}, 
            temp.{nameof(CustardEntity.PhoneNumber2)}, 
            temp.{nameof(CustardEntity.Date)}, 
            temp.{nameof(CustardEntity.UnixDate)}, 
            temp.{nameof(CustardEntity.UnixCancelDate)}
        FROM {EntityDetails.TempTable} temp
        WHERE NOT EXISTS (
            SELECT 1 
            FROM {TableNames.CustardEntitiesName} t
            WHERE t.{nameof(CustardEntity.Id)} = temp.{nameof(CustardEntity.Id)}
        );
    """;

    protected override bool IsUpdatable => true;

    private static int[]? _columnIndexes;
    protected override int[] ColumnIndexes => _columnIndexes ??= [.. Enumerable.Range(0, EntityDetails.ColumnCount)];
    protected override void InsertBatch(List<CustardEntity> batch)
    {
#pragma warning disable CS8604
        var values = new List<object>();
        var rows = new List<string>();

        for (int i = 0; i < batch.Count; i++)
        {
            var e = batch[i];
            int offset = i * EntityDetails.ColumnCount;

            var placeholders = ColumnIndexes.Select(ci => $"{{{offset + ci}}}");
            rows.Add($"({string.Join(", ", placeholders)})");

            // Order here must match order below
            values.Add(e.Id);
            values.Add(e.Active ? 1 : 0);
            values.Add(e.PhoneNumber.Number);
            values.Add(e.PhoneNumber2?.Number); // Null is fine because we're executing Sql. DON'T USE DBNull.Value. It's a .net thing, not a sql thing
            values.Add(e.Date.ToString(IsoString));
            values.Add(e.UnixDate);
            values.Add(e.UnixCancelDate);
        }

        // Order here must match order above
        string sql = $"""
            INSERT INTO {EntityDetails.TempTable} (
                {nameof(CustardEntity.Id)},
                {nameof(CustardEntity.Active)},
                {nameof(CustardEntity.PhoneNumber)},
                {nameof(CustardEntity.PhoneNumber2)},
                {nameof(CustardEntity.Date)},
                {nameof(CustardEntity.UnixDate)},
                {nameof(CustardEntity.UnixCancelDate)}
            )
            VALUES {string.Join(",", rows)};
        """;
        _context.Database.ExecuteSqlRaw(sql, [.. values]);
#pragma warning restore CS8604
    }

    public override async Task<Result<List<CustardEntity>>> UpsertRangeAsync(
        List<CustardEntity> entities,
        CancellationToken ct = default) => await UpsertEntityRangeAsync(entities, ct);

}
