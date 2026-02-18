using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class CaliperRepository
    (
        PlumbingContext context,
        ILogger<CaliperRepository> logger
    ) : PlumbingContextEntityRepository<CaliperEntity, CaliperRepository>(context, logger), IRepository<CaliperEntity>
{
    protected override IQueryable<CaliperEntity> WithIncludes(IQueryable<CaliperEntity> q)
    {
        return q
            .Include(c => c.CustardCaliperLinks)
            .Include(c => c.SandCaliperLinks)
            .Include(c => c.PlumbingCaliperLinks)
            .Include(c => c.CornCaliperLinks);
    }

    protected override UpsertFields EntityDetails => new(
        TableName: TableNames.CaliperEntitiesName,
        TempTable: $"temp_{TableNames.CaliperEntitiesName}",
        EntityName: nameof(CaliperEntity),
        ColumnCount: 9);

    protected override string CreateTempTable => $"""
        CREATE TEMP TABLE IF NOT EXISTS {EntityDetails.TempTable} (
            {nameof(CaliperEntity.Id)} INTEGER PRIMARY KEY,
            {nameof(CaliperEntity.PhoneNumber)} INTEGER NOT NULL,
            {nameof(CaliperEntity.Date)} TEXT NOT NULL,
            {nameof(CaliperEntity.UnixDate)} INTEGER NOT NULL,
            {nameof(CaliperEntity.Note)} TEXT,
            {nameof(CaliperEntity.Source)} TEXT,
            {nameof(CaliperEntity.Location)} TEXT,
            {nameof(CaliperEntity.Duration)} INTEGER,
            {nameof(CaliperEntity.Billable)} INTEGER
        ) WITHOUT ROWID;
        DELETE FROM {EntityDetails.TempTable};
    """;

    protected override string UpdateSql => $"""
        UPDATE {TableNames.CaliperEntitiesName}
        SET
            {nameof(CaliperEntity.PhoneNumber)} = (
                SELECT temp.{nameof(CaliperEntity.PhoneNumber)}
                FROM {EntityDetails.TempTable} temp
                WHERE temp.{nameof(CaliperEntity.Id)} = {TableNames.CaliperEntitiesName}.{nameof(CaliperEntity.Id)}
            ),
            {nameof(CaliperEntity.Date)} = (
                SELECT temp.{nameof(CaliperEntity.Date)}
                FROM {EntityDetails.TempTable} temp
                WHERE temp.{nameof(CaliperEntity.Id)} = {TableNames.CaliperEntitiesName}.{nameof(CaliperEntity.Id)}
            ),
            {nameof(CaliperEntity.UnixDate)} = (
                SELECT temp.{nameof(CaliperEntity.UnixDate)}
                FROM {EntityDetails.TempTable} temp
                WHERE temp.{nameof(CaliperEntity.Id)} = {TableNames.CaliperEntitiesName}.{nameof(CaliperEntity.Id)}
            ),
            {nameof(CaliperEntity.Note)} = (
                SELECT temp.{nameof(CaliperEntity.Note)}
                FROM {EntityDetails.TempTable} temp
                WHERE temp.{nameof(CaliperEntity.Id)} = {TableNames.CaliperEntitiesName}.{nameof(CaliperEntity.Id)}
            ),
            {nameof(CaliperEntity.Source)} = (
                SELECT temp.{nameof(CaliperEntity.Source)}
                FROM {EntityDetails.TempTable} temp
                WHERE temp.{nameof(CaliperEntity.Id)} = {TableNames.CaliperEntitiesName}.{nameof(CaliperEntity.Id)}
            ),
            {nameof(CaliperEntity.Location)} = (
                SELECT temp.{nameof(CaliperEntity.Location)}
                FROM {EntityDetails.TempTable} temp
                WHERE temp.{nameof(CaliperEntity.Id)} = {TableNames.CaliperEntitiesName}.{nameof(CaliperEntity.Id)}
            ),
            {nameof(CaliperEntity.Duration)} = (
                SELECT temp.{nameof(CaliperEntity.Duration)}
                FROM {EntityDetails.TempTable} temp
                WHERE temp.{nameof(CaliperEntity.Id)} = {TableNames.CaliperEntitiesName}.{nameof(CaliperEntity.Id)}
            ),
            {nameof(CaliperEntity.Billable)} = (
                SELECT temp.{nameof(CaliperEntity.Billable)}
                FROM {EntityDetails.TempTable} temp
                WHERE temp.{nameof(CaliperEntity.Id)} = {TableNames.CaliperEntitiesName}.{nameof(CaliperEntity.Id)}
            )
        WHERE EXISTS (
            SELECT 1
            FROM {EntityDetails.TempTable} temp
            WHERE temp.{nameof(CaliperEntity.Id)} = {TableNames.CaliperEntitiesName}.{nameof(CaliperEntity.Id)}
        );
    """;

    protected override string InsertSql => $"""
        INSERT INTO {TableNames.CaliperEntitiesName} (
            {nameof(CaliperEntity.Id)}, 
            {nameof(CaliperEntity.PhoneNumber)}, 
            {nameof(CaliperEntity.Date)}, 
            {nameof(CaliperEntity.UnixDate)}, 
            {nameof(CaliperEntity.Note)}, 
            {nameof(CaliperEntity.Source)}, 
            {nameof(CaliperEntity.Location)}, 
            {nameof(CaliperEntity.Duration)}, 
            {nameof(CaliperEntity.Billable)}
        )
        SELECT
            {nameof(CaliperEntity.Id)}, 
            {nameof(CaliperEntity.PhoneNumber)}, 
            {nameof(CaliperEntity.Date)}, 
            {nameof(CaliperEntity.UnixDate)}, 
            {nameof(CaliperEntity.Note)}, 
            {nameof(CaliperEntity.Source)}, 
            {nameof(CaliperEntity.Location)}, 
            {nameof(CaliperEntity.Duration)}, 
            {nameof(CaliperEntity.Billable)}
        FROM {EntityDetails.TempTable} temp
        WHERE NOT EXISTS (
            SELECT 1
            FROM {TableNames.CaliperEntitiesName} t
            WHERE t.{nameof(CaliperEntity.Id)} = temp.{nameof(CaliperEntity.Id)}
        );
    """;
    protected override bool IsUpdatable => true;

    protected override void InsertBatch(List<CaliperEntity> batch)
    {
        var values = new List<object>();
        var rows = new List<string>();

        for (int i = 0; i < batch.Count; i++)
        {
            var e = batch[i];
            int offset = i * EntityDetails.ColumnCount;

            // Build placeholder string: ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})
            rows.Add($"({{{offset}}}, {{{offset + 1}}}, {{{offset + 2}}}, {{{offset + 3}}}, {{{offset + 4}}}, {{{offset + 5}}}, {{{offset + 6}}}, {{{offset + 7}}}, {{{offset + 8}}})");

            values.Add(e.Id);
            values.Add(e.PhoneNumber.Number); // Extract long from PhoneNumber object
            values.Add(e.Date.ToString("yyyy-MM-dd HH:mm:ss")); // ISO String for SQLite
            values.Add(e.UnixDate);
            values.Add(e.Note);
            values.Add(e.Source);
            values.Add(e.Location);
            values.Add(e.Duration);
            values.Add(e.Billable ? 1 : 0);
        }

        string sql = $"INSERT INTO {EntityDetails.TempTable} VALUES {string.Join(",", rows)};";
        _context.Database.ExecuteSqlRaw(sql, [.. values]);
    }

    public override async Task<Result<List<CaliperEntity>>> UpsertRangeAsync(
        List<CaliperEntity> entities, 
        CancellationToken ct = default) => await UpsertEntityRangeAsync(entities, ct);

}
