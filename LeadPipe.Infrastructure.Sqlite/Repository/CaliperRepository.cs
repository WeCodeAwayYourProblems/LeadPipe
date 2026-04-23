using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Database.Configuration;
using LeadPipe.Infrastructure.Entity;
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

    protected override UpsertFields EntityDetails { get; } =
        new(
            TableName: TableNames.CaliperEntitiesName,
            TempTable: $"temp_{TableNames.CaliperEntitiesName}",
            EntityName: nameof(CaliperEntity),
            ColumnCount: 10);

    protected override string CreateTempTable => $"""
        CREATE TEMP TABLE IF NOT EXISTS {EntityDetails.TempTable} (
            {nameof(CaliperEntity.Id)} INTEGER PRIMARY KEY,
            {nameof(CaliperEntity.PhoneNumber)} INTEGER NOT NULL,
            {nameof(CaliperEntity.Date)} TEXT NOT NULL,
            {nameof(CaliperEntity.UnixDate)} INTEGER NOT NULL,
            {nameof(CaliperEntity.Note)} TEXT,
            {nameof(CaliperEntity.Source)} TEXT,
            {nameof(CaliperEntity.Label)} TEXT,
            {nameof(CaliperEntity.Location)} TEXT,
            {nameof(CaliperEntity.Duration)} INTEGER,
            {nameof(CaliperEntity.Billable)} INTEGER
        ) WITHOUT ROWID;
        DELETE FROM {EntityDetails.TempTable};
    """;

    protected override string UpdateSql => $"""
        UPDATE {TableNames.CaliperEntitiesName}
        SET
            {nameof(CaliperEntity.PhoneNumber)} = temp.{nameof(CaliperEntity.PhoneNumber)},
            {nameof(CaliperEntity.Date)} = temp.{nameof(CaliperEntity.Date)},
            {nameof(CaliperEntity.UnixDate)} = temp.{nameof(CaliperEntity.UnixDate)},
            {nameof(CaliperEntity.Note)} = temp.{nameof(CaliperEntity.Note)},
            {nameof(CaliperEntity.Source)} = temp.{nameof(CaliperEntity.Source)},
            {nameof(CaliperEntity.Label)} = temp.{nameof(CaliperEntity.Label)},
            {nameof(CaliperEntity.Location)} = temp.{nameof(CaliperEntity.Location)},
            {nameof(CaliperEntity.Duration)} = temp.{nameof(CaliperEntity.Duration)},
            {nameof(CaliperEntity.Billable)} = temp.{nameof(CaliperEntity.Billable)}
        FROM {EntityDetails.TempTable} temp
        WHERE {TableNames.CaliperEntitiesName}.{nameof(CaliperEntity.Id)} = temp.{nameof(CaliperEntity.Id)};
    """;


    protected override string InsertSql => $"""
        INSERT INTO {TableNames.CaliperEntitiesName} (
            {nameof(CaliperEntity.Id)}, 
            {nameof(CaliperEntity.PhoneNumber)}, 
            {nameof(CaliperEntity.Date)}, 
            {nameof(CaliperEntity.UnixDate)}, 
            {nameof(CaliperEntity.Note)}, 
            {nameof(CaliperEntity.Source)}, 
            {nameof(CaliperEntity.Label)},
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
            {nameof(CaliperEntity.Label)},
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

    private static int[]? _columnIndexes;
    protected override int[] ColumnIndexes => _columnIndexes ??= [.. Enumerable.Range(0, EntityDetails.ColumnCount)];
    protected override void InsertBatch(List<CaliperEntity> batch)
    {
        var values = new List<object>();
        var rows = new List<string>();

        for (int i = 0; i < batch.Count; i++)
        {
            var e = batch[i];
            int offset = i * EntityDetails.ColumnCount;

            var placeholders = ColumnIndexes.Select(columnIndex => $"{{{offset + columnIndex}}}");
            rows.Add($"({string.Join(", ", placeholders)})");

            // Order here must match order below
            values.Add(e.Id);
            values.Add(e.PhoneNumber.Number); // Extract long from PhoneNumber object
            values.Add(e.Date.ToString(IsoString));
            values.Add(e.UnixDate);
            values.Add(e.Note);
            values.Add(e.Source);
            values.Add(e.Label);
            values.Add(e.Location);
            values.Add(e.Duration);
            values.Add(e.Billable ? 1 : 0);
        }

        // Order here must match order above
        string sql = $"""
        INSERT INTO {EntityDetails.TempTable} (
            {nameof(CaliperEntity.Id)},
            {nameof(CaliperEntity.PhoneNumber)},
            {nameof(CaliperEntity.Date)},
            {nameof(CaliperEntity.UnixDate)},
            {nameof(CaliperEntity.Note)},
            {nameof(CaliperEntity.Source)},
            {nameof(CaliperEntity.Label)},
            {nameof(CaliperEntity.Location)},
            {nameof(CaliperEntity.Duration)},
            {nameof(CaliperEntity.Billable)}
        )
        VALUES {string.Join(",", rows)};
        """;

        _context.Database.ExecuteSqlRaw(sql, [.. values]);
    }

    public override async Task<Result<List<CaliperEntity>>> UpsertRangeAsync(
        List<CaliperEntity> entities,
        CancellationToken ct = default) => await UpsertEntityRangeAsync(entities, ct);

}
