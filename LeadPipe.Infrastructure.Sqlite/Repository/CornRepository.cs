using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class CornRepository(
    PlumbingContext context,
    ILogger<CornRepository> logger
) : PlumbingContextEntityRepository<CornEntity, CornRepository>(context, logger), IRepository<CornEntity>
{
    protected override IQueryable<CornEntity> WithIncludes(IQueryable<CornEntity> q)
    {
        return q
            .Include(c => c.CustardCornLinks)
            .Include(c => c.SandCornLinks)
            .Include(c => c.CornCaliperLinks)
            .Include(c => c.CornPlumbingLinks);
    }

    protected override void InsertBatch(List<CornEntity> batch)
    {
        var values = new List<object>();
        var rows = new List<string>();
        const int colsPerRow = 7;

        for (int i = 0; i < batch.Count; i++)
        {
            var e = batch[i];
            int offset = i * colsPerRow;

            // Build placeholder string: ({0}, {1}, {2}, {3}, {4}, {5}, {6})
            rows.Add($"({{{offset}}}, {{{offset + 1}}}, {{{offset + 2}}}, {{{offset + 3}}}, {{{offset + 4}}}, {{{offset + 5}}}, {{{offset + 6}}})");

            values.Add(e.Id);
            values.Add(e.PhoneNumber.Number);
            values.Add(e.Date.ToString("yyyy-MM-dd HH:mm:ss"));
            values.Add(e.UnixDate);
            values.Add(e.Payload ?? string.Empty);
            values.Add(e.MetaData ?? string.Empty);
            values.Add(e.Source ?? string.Empty);
        }

        string sql = $"INSERT INTO {EntityDetails.TempTable} VALUES {string.Join(",", rows)};";
        _context.Database.ExecuteSqlRaw(sql, [.. values]);
    }
    protected override UpsertFields EntityDetails => new(
        TableName: TableNames.CornEntitiesName,
        TempTable: $"temp_{TableNames.CornEntitiesName}",
        EntityName: nameof(CornEntity),
        ColumnCount: 7);
    protected override string CreateTempTable => $"""
        CREATE TEMP TABLE IF NOT EXISTS {EntityDetails.TempTable} (
            {nameof(CornEntity.Id)} INTEGER PRIMARY KEY,
            {nameof(CornEntity.PhoneNumber)} INTEGER NOT NULL,
            {nameof(CornEntity.Date)} TEXT NOT NULL,
            {nameof(CornEntity.UnixDate)} INTEGER NOT NULL,
            {nameof(CornEntity.Payload)} TEXT NOT NULL,
            {nameof(CornEntity.MetaData)} TEXT NOT NULL,
            {nameof(CornEntity.Source)} TEXT NOT NULL
        ) WITHOUT ROWID;
        DELETE FROM {EntityDetails.TempTable};
    """;
    protected override string UpdateSql => $"""
        UPDATE {TableNames.CornEntitiesName}
        SET 
            {nameof(CornEntity.PhoneNumber)} = temp.{nameof(CornEntity.PhoneNumber)},
            {nameof(CornEntity.Date)} = temp.{nameof(CornEntity.Date)},
            {nameof(CornEntity.UnixDate)} = temp.{nameof(CornEntity.UnixDate)},
            {nameof(CornEntity.Payload)} = temp.{nameof(CornEntity.Payload)},
            {nameof(CornEntity.MetaData)} = temp.{nameof(CornEntity.MetaData)},
            {nameof(CornEntity.Source)} = temp.{nameof(CornEntity.Source)}
        FROM {EntityDetails.TempTable} temp
        WHERE {TableNames.CornEntitiesName}.{nameof(CornEntity.Id)} = temp.{nameof(CornEntity.Id)};
    """;
    protected override string InsertSql => $"""
        INSERT INTO {TableNames.CornEntitiesName} 
        (
            {nameof(CornEntity.Id)}, 
            {nameof(CornEntity.PhoneNumber)}, 
            {nameof(CornEntity.Date)}, 
            {nameof(CornEntity.UnixDate)}, 
            {nameof(CornEntity.Payload)}, 
            {nameof(CornEntity.MetaData)},  
            {nameof(CornEntity.Source)}
        )
        SELECT 
            temp.{nameof(CornEntity.Id)}, 
            temp.{nameof(CornEntity.PhoneNumber)}, 
            temp.{nameof(CornEntity.Date)}, 
            temp.{nameof(CornEntity.UnixDate)}, 
            temp.{nameof(CornEntity.Payload)},     
            temp.{nameof(CornEntity.MetaData)}, 
            temp.{nameof(CornEntity.Source)}
        FROM {EntityDetails.TempTable} temp
        WHERE NOT EXISTS (
            SELECT 1 
            FROM {TableNames.CornEntitiesName} t
            WHERE t.{nameof(CornEntity.Id)} = temp.{nameof(CornEntity.Id)}
        );
    """;

    public override async Task<Result<List<CornEntity>>> UpsertRangeAsync(
        List<CornEntity> entities,
        CancellationToken ct = default) => await UpsertEntityRangeAsync(entities, ct);

}
