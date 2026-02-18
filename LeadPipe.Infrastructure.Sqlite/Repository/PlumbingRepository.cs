using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public class PlumbingRepository
    (
        PlumbingContext context,
        ILogger<PlumbingRepository> logger
    ) : PlumbingEntityContextRepository<PlumbingEntity, PlumbingRepository>(context, logger), IRepository<PlumbingEntity>
{
    protected override IQueryable<PlumbingEntity> WithIncludes(IQueryable<PlumbingEntity> q)
    {
        return q
            .Include(c => c.CustardPlumbingLinks)
            .Include(c => c.SandPlumbingLinks)
            .Include(c => c.PlumbingCaliperLinks)
            .Include(c => c.CornPlumbingLinks);
    }
    
    protected override void InsertBatch(List<PlumbingEntity> batch)
    {
        var values = new List<object>();
        var rows = new List<string>();
        const int cols = 6;

        for (int i = 0; i < batch.Count; i++)
        {
            var e = batch[i];
            int o = i * cols;
            rows.Add($"({{{o}}},{{{o + 1}}},{{{o + 2}}},{{{o + 3}}},{{{o + 4}}},{{{o + 5}}})");

            values.Add(e.PhoneNumber.Number);
            values.Add(e.Date.ToString("yyyy-MM-dd HH:mm:ss"));
            values.Add(e.UnixDate);
            values.Add(e.Contents ?? (object)DBNull.Value);
            values.Add(e.Source.ToString()); // Ensure Enum is passed as string
            values.Add(e.MetaData ?? string.Empty);
        }
        string joined = $"INSERT INTO {EntityDetails.TempTable} VALUES {string.Join(",", rows)}";
        _context.Database.ExecuteSqlRaw(joined, [.. values]);
    }

    protected override UpsertFields EntityDetails => throw new NotImplementedException();

    protected override string CreateTempTable => $"""
        CREATE TEMP TABLE IF NOT EXISTS {EntityDetails.TempTable} (
            {nameof(PlumbingEntity.PhoneNumber)} INTEGER NOT NULL,
            {nameof(PlumbingEntity.Date)} TEXT NOT NULL,
            {nameof(PlumbingEntity.UnixDate)} INTEGER NOT NULL,
            {nameof(PlumbingEntity.Contents)} TEXT,
            {nameof(PlumbingEntity.Source)} TEXT NOT NULL,
            {nameof(PlumbingEntity.MetaData)} TEXT NOT NULL,
            PRIMARY KEY ({nameof(PlumbingEntity.PhoneNumber)}, {nameof(PlumbingEntity.Date)}, {nameof(PlumbingEntity.Source)})
        ) WITHOUT ROWID;
        DELETE FROM {EntityDetails.TempTable};
    """;

    protected override string UpdateSql => $"""
        UPDATE {TableNames.PlumbingEntitiesName}
        SET
            {TableNames.PlumbingEntitiesName}.{nameof(PlumbingEntity.UnixDate)} = temp.{nameof(PlumbingEntity.UnixDate)},
            {TableNames.PlumbingEntitiesName}.{nameof(PlumbingEntity.Contents)} = temp.{nameof(PlumbingEntity.Contents)},
            {TableNames.PlumbingEntitiesName}.{nameof(PlumbingEntity.MetaData)} = temp.{nameof(PlumbingEntity.MetaData)}
        FROM {EntityDetails.TempTable} temp
        WHERE {TableNames.PlumbingEntitiesName}.{nameof(PlumbingEntity.PhoneNumber)} = temp.{nameof(PlumbingEntity.PhoneNumber)}
          AND {TableNames.PlumbingEntitiesName}.{nameof(PlumbingEntity.Date)} = temp.{nameof(PlumbingEntity.Date)}
          AND {TableNames.PlumbingEntitiesName}.{nameof(PlumbingEntity.Source)} = temp.{nameof(PlumbingEntity.Source)};
    """;

    protected override string InsertSql => $"""
        INSERT INTO {TableNames.PlumbingEntitiesName} (
            {nameof(PlumbingEntity.PhoneNumber)},
            {nameof(PlumbingEntity.Date)},
            {nameof(PlumbingEntity.UnixDate)},
            {nameof(PlumbingEntity.Contents)},
            {nameof(PlumbingEntity.Source)},
            {nameof(PlumbingEntity.MetaData)}
        )
        SELECT
            temp.{nameof(PlumbingEntity.PhoneNumber)},
            temp.{nameof(PlumbingEntity.Date)},
            temp.{nameof(PlumbingEntity.UnixDate)},
            temp.{nameof(PlumbingEntity.Contents)},
            temp.{nameof(PlumbingEntity.Source)},
            temp.{nameof(PlumbingEntity.MetaData)}
        FROM {EntityDetails.TempTable} temp
        WHERE NOT EXISTS (
            SELECT 1
            FROM {TableNames.PlumbingEntitiesName} t
            WHERE t.{nameof(PlumbingEntity.PhoneNumber)} = temp.{nameof(PlumbingEntity.PhoneNumber)}
              AND t.{nameof(PlumbingEntity.Date)} = temp.{nameof(PlumbingEntity.Date)}
              AND t.{nameof(PlumbingEntity.Source)} = temp.{nameof(PlumbingEntity.Source)}
        );
    """;


    public override async Task<Result<List<PlumbingEntity>>> UpsertRangeAsync(
        List<PlumbingEntity> entities,
        CancellationToken ct = default) => await UpsertEntityRangeAsync(entities, ct);

}
