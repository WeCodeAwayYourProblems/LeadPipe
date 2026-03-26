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
    ) : PlumbingContextEntityRepository<PlumbingEntity, PlumbingRepository>(context, logger), IRepository<PlumbingEntity>
{
    protected override IQueryable<PlumbingEntity> WithIncludes(IQueryable<PlumbingEntity> q)
    {
        return q
            .Include(c => c.CustardPlumbingLinks)
            .Include(c => c.SandPlumbingLinks)
            .Include(c => c.PlumbingCaliperLinks)
            .Include(c => c.CornPlumbingLinks)
            .Include(c => c.PhoneNumbers);
    }

    protected override UpsertFields EntityDetails { get; } =
    new(
        TableName: TableNames.PlumbingEntitiesName,
        TempTable: $"temp_{TableNames.PlumbingEntitiesName}",
        EntityName: nameof(PlumbingEntity),
        ColumnCount: 7
    );

    protected override string CreateTempTable => $"""
        CREATE TEMP TABLE IF NOT EXISTS {EntityDetails.TempTable} (
            {nameof(PlumbingEntity.PhoneNumber)} INTEGER NOT NULL,
            {nameof(PlumbingEntity.Date)} TEXT NOT NULL,
            {nameof(PlumbingEntity.UnixDate)} INTEGER NOT NULL,
            {nameof(PlumbingEntity.Contents)} TEXT,
            {nameof(PlumbingEntity.Source)} TEXT NOT NULL,
            {nameof(PlumbingEntity.MetaData)} TEXT NOT NULL,
            {nameof(PlumbingEntity.Branch)} TEXT,
            PRIMARY KEY (
                {nameof(PlumbingEntity.PhoneNumber)},
                {nameof(PlumbingEntity.UnixDate)}, 
                {nameof(PlumbingEntity.Source)}, 
                {nameof(PlumbingEntity.MetaData)}
            )
        ) WITHOUT ROWID;
        DELETE FROM {EntityDetails.TempTable};
    """;

    protected override string UpdateSql => $"""
        UPDATE {TableNames.PlumbingEntitiesName}
        SET
            {nameof(PlumbingEntity.Contents)} = temp.{nameof(PlumbingEntity.Contents)},
            {nameof(PlumbingEntity.Branch)} = temp.{nameof(PlumbingEntity.Branch)}
        FROM {EntityDetails.TempTable} temp
        WHERE {TableNames.PlumbingEntitiesName}.{nameof(PlumbingEntity.PhoneNumber)} = temp.{nameof(PlumbingEntity.PhoneNumber)}
          AND {TableNames.PlumbingEntitiesName}.{nameof(PlumbingEntity.UnixDate)} = temp.{nameof(PlumbingEntity.UnixDate)}
          AND {TableNames.PlumbingEntitiesName}.{nameof(PlumbingEntity.Source)} = temp.{nameof(PlumbingEntity.Source)}
          AND {TableNames.PlumbingEntitiesName}.{nameof(PlumbingEntity.MetaData)} = temp.{nameof(PlumbingEntity.MetaData)};
    """;

    protected override string InsertSql => $"""
        INSERT INTO {TableNames.PlumbingEntitiesName} 
        (
            {nameof(PlumbingEntity.PhoneNumber)},
            {nameof(PlumbingEntity.Date)},
            {nameof(PlumbingEntity.UnixDate)},
            {nameof(PlumbingEntity.Contents)},
            {nameof(PlumbingEntity.Source)},
            {nameof(PlumbingEntity.MetaData)},
            {nameof(PlumbingEntity.Branch)}
        )
        SELECT
            temp.{nameof(PlumbingEntity.PhoneNumber)},
            temp.{nameof(PlumbingEntity.Date)},
            temp.{nameof(PlumbingEntity.UnixDate)},
            temp.{nameof(PlumbingEntity.Contents)},
            temp.{nameof(PlumbingEntity.Source)},
            temp.{nameof(PlumbingEntity.MetaData)},
            temp.{nameof(PlumbingEntity.Branch)}
        FROM {EntityDetails.TempTable} temp
        WHERE NOT EXISTS (
            SELECT 1
            FROM {TableNames.PlumbingEntitiesName} t
            WHERE t.{nameof(PlumbingEntity.PhoneNumber)} = temp.{nameof(PlumbingEntity.PhoneNumber)}
              AND t.{nameof(PlumbingEntity.UnixDate)} = temp.{nameof(PlumbingEntity.UnixDate)}
              AND t.{nameof(PlumbingEntity.Source)} = temp.{nameof(PlumbingEntity.Source)}
              AND t.{nameof(PlumbingEntity.MetaData)} = temp.{nameof(PlumbingEntity.MetaData)}
        );
    """;

    protected override bool IsUpdatable => true;

    private static int[]? _columnIndexes;
    protected override int[] ColumnIndexes => _columnIndexes ??= [.. Enumerable.Range(0, EntityDetails.ColumnCount)];
    protected override void InsertBatch(List<PlumbingEntity> batch)
    {
#pragma warning disable CS8604
        var values = new List<object>();
        var rows = new List<string>();

        for (int i = 0; i < batch.Count; i++)
        {
            var e = batch[i];
            int o = i * EntityDetails.ColumnCount;
            var placeholders = ColumnIndexes.Select(ci => $"{{{o + ci}}}");
            rows.Add($"({string.Join(", ", placeholders)})");

            // Order here must match order below
            values.Add(e.PhoneNumber.Number);
            values.Add(e.Date.ToString(IsoString));
            values.Add(e.UnixDate);
            values.Add(e.Contents);
            values.Add(e.Source.ToString()); // Ensure Enum is passed as string
            values.Add(e.MetaData ?? string.Empty);
            values.Add(e.Branch);
        }

        // Order here must match order above
        string joined = $"""
            INSERT INTO {EntityDetails.TempTable} (
                {nameof(PlumbingEntity.PhoneNumber)},
                {nameof(PlumbingEntity.Date)},
                {nameof(PlumbingEntity.UnixDate)},
                {nameof(PlumbingEntity.Contents)},
                {nameof(PlumbingEntity.Source)},
                {nameof(PlumbingEntity.MetaData)},
                {nameof(PlumbingEntity.Branch)}
            )
            VALUES {string.Join(",", rows)}
         """;

        _context.Database.ExecuteSqlRaw(joined, [.. values]);
#pragma warning restore CS8604
    }

    public override async Task<Result<List<PlumbingEntity>>> UpsertRangeAsync(
        List<PlumbingEntity> entities,
        CancellationToken ct = default) => await UpsertEntityRangeAsync(entities, ct);

}
