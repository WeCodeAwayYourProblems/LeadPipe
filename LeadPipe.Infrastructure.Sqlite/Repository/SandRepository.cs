using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class SandRepository
    (
        PlumbingContext context,
        ILogger<SandRepository> logger
    ) : PlumbingContextEntityRepository<SandEntity, SandRepository>(context, logger), IRepository<SandEntity>
{
    protected override IQueryable<SandEntity> WithIncludes(IQueryable<SandEntity> q)
    {
        return q
            .Include(c => c.CustardEntity)
            .Include(c => c.SandPlumbingLinks)
            .Include(c => c.SandCaliperLinks)
            .Include(c => c.SandCornLinks);
    }

    protected override void InsertBatch(List<SandEntity> batch)
    {
#pragma warning disable CS8604
        var values = new List<object>();
        var rows = new List<string>();

        for (int i = 0; i < batch.Count; i++)
        {
            var e = batch[i];
            int o = i * EntityDetails.ColumnCount;
            rows.Add($"({{{o}}},{{{o + 1}}},{{{o + 2}}},{{{o + 3}}},{{{o + 4}}},{{{o + 5}}},{{{o + 6}}},{{{o + 7}}},{{{o + 8}}},{{{o + 9}}},{{{o + 10}}},{{{o + 11}}},{{{o + 12}}},{{{o + 13}}})");

            values.Add(e.Id);
            values.Add(e.CustardId);
            values.Add(e.Date.ToString("yyyy-MM-dd HH:mm:ss"));
            values.Add(e.UnixDate);
            values.Add(e.CancelDate == default ? null : e.CancelDate.ToString("yyyy-MM-dd HH:mm:ss"));
            values.Add(e.UnixCancelDate);
            values.Add(e.Active ? 1 : 0);
            values.Add(e.Complete ? 1 : 0);
            values.Add(e.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            values.Add(e.Type);
            values.Add(e.Seller);
            values.Add(e.Seller2);
            values.Add(e.Seller3);
            values.Add(e.Offerman ?? string.Empty);
        }

        string joined = $"INSERT INTO {EntityDetails.TempTable} VALUES {string.Join(",", rows)};";
        _context.Database.ExecuteSqlRaw(joined, [.. values]);
#pragma warning restore CS8604
    }

    protected override UpsertFields EntityDetails => new(
        TableName: TableNames.SandEntitiesName,
        TempTable: $"temp_{TableNames.SandEntitiesName}",
        EntityName: nameof(SandEntity),
        ColumnCount: 14
        );

    protected override string CreateTempTable => $"""
        CREATE TEMP TABLE IF NOT EXISTS {EntityDetails.TempTable} (
            {nameof(SandEntity.Id)} INTEGER PRIMARY KEY,
            {nameof(SandEntity.CustardId)} INTEGER NOT NULL,
            {nameof(SandEntity.Date)} TEXT,
            {nameof(SandEntity.UnixDate)} INTEGER,
            {nameof(SandEntity.CancelDate)} TEXT,
            {nameof(SandEntity.UnixCancelDate)} INTEGER,
            {nameof(SandEntity.Active)} INTEGER,
            {nameof(SandEntity.Complete)} INTEGER,
            {nameof(SandEntity.Value)} TEXT,
            {nameof(SandEntity.Type)} TEXT,
            {nameof(SandEntity.Seller)} INTEGER,
            {nameof(SandEntity.Seller2)} INTEGER,
            {nameof(SandEntity.Seller3)} INTEGER,
            {nameof(SandEntity.Offerman)} TEXT NOT NULL
        ) WITHOUT ROWID;
        DELETE FROM {EntityDetails.TempTable};
    """;

    protected override string UpdateSql => $"""
        UPDATE {TableNames.SandEntitiesName}
        SET
            {nameof(SandEntity.CustardId)} = temp.{nameof(SandEntity.CustardId)},
            {nameof(SandEntity.Date)} = temp.{nameof(SandEntity.Date)},
            {nameof(SandEntity.UnixDate)} = temp.{nameof(SandEntity.UnixDate)},
            {nameof(SandEntity.CancelDate)} = temp.{nameof(SandEntity.CancelDate)},
            {nameof(SandEntity.UnixCancelDate)} = temp.{nameof(SandEntity.UnixCancelDate)},
            {nameof(SandEntity.Active)} = temp.{nameof(SandEntity.Active)},
            {nameof(SandEntity.Complete)} = temp.{nameof(SandEntity.Complete)},
            {nameof(SandEntity.Value)} = temp.{nameof(SandEntity.Value)},
            {nameof(SandEntity.Type)} = temp.{nameof(SandEntity.Type)},
            {nameof(SandEntity.Seller)} = temp.{nameof(SandEntity.Seller)},
            {nameof(SandEntity.Seller2)} = temp.{nameof(SandEntity.Seller2)},
            {nameof(SandEntity.Seller3)} = temp.{nameof(SandEntity.Seller3)},
            {nameof(SandEntity.Offerman)} = temp.{nameof(SandEntity.Offerman)}
        FROM {EntityDetails.TempTable} temp
        WHERE {TableNames.SandEntitiesName}.{nameof(SandEntity.Id)} = temp.{nameof(SandEntity.Id)};
    """;

    protected override string InsertSql => $"""
        INSERT INTO {TableNames.SandEntitiesName} (
            {nameof(SandEntity.Id)}, 
            {nameof(SandEntity.CustardId)}, 
            {nameof(SandEntity.Date)}, 
            {nameof(SandEntity.UnixDate)}, 
            {nameof(SandEntity.CancelDate)}, 
            {nameof(SandEntity.UnixCancelDate)}, 
            {nameof(SandEntity.Active)}, 
            {nameof(SandEntity.Complete)}, 
            {nameof(SandEntity.Value)}, 
            {nameof(SandEntity.Type)}, 
            {nameof(SandEntity.Seller)}, 
            {nameof(SandEntity.Seller2)}, 
            {nameof(SandEntity.Seller3)}, 
            {nameof(SandEntity.Offerman)}
        )
        SELECT 
            temp.{nameof(SandEntity.Id)}, 
            temp.{nameof(SandEntity.CustardId)}, 
            temp.{nameof(SandEntity.Date)}, 
            temp.{nameof(SandEntity.UnixDate)}, 
            temp.{nameof(SandEntity.CancelDate)}, 
            temp.{nameof(SandEntity.UnixCancelDate)}, 
            temp.{nameof(SandEntity.Active)}, 
            temp.{nameof(SandEntity.Complete)}, 
            temp.{nameof(SandEntity.Value)}, 
            temp.{nameof(SandEntity.Type)}, 
            temp.{nameof(SandEntity.Seller)}, 
            temp.{nameof(SandEntity.Seller2)}, 
            temp.{nameof(SandEntity.Seller3)}, 
            temp.{nameof(SandEntity.Offerman)}
        FROM {EntityDetails.TempTable} temp
        WHERE NOT EXISTS (
            SELECT 1 
            FROM {TableNames.SandEntitiesName} t
            WHERE t.{nameof(SandEntity.Id)} = temp.{nameof(SandEntity.Id)}
        );
    """;
    protected override bool IsUpdatable => true;
    public override async Task<Result<List<SandEntity>>> UpsertRangeAsync(
        List<SandEntity> entities,
        CancellationToken ct = default)
    {
        var validAndDeduplicated = SandValidityAndDeduplication(entities);
        var result = await UpsertEntityRangeAsync(validAndDeduplicated, ct);
        return result;
    }

    private List<SandEntity> SandValidityAndDeduplication(List<SandEntity> entities)
    {
        List<SandEntity> uniqueEntities =
        [
            .. entities
                .GroupBy(e => e.Id)
                .Select(g => g.Last())
        ];

        HashSet<long> neededCustardIds = [.. uniqueEntities.Select(e => e.CustardId)];
        HashSet<long> existingCustardIds = [.. _context.CustardEntities
            .Where(c => neededCustardIds.Contains(c.Id)).Select(c => c.Id)];

        List<SandEntity> validEntities = new(uniqueEntities.Count);
        List<SandEntity> rejectedEntities = new(uniqueEntities.Count);

        foreach (var e in uniqueEntities)
        {
            if (existingCustardIds.Contains(e.CustardId))
                validEntities.Add(e);
            else
                rejectedEntities.Add(e);
        }

        int rejected = rejectedEntities.Count;

        if (rejected > 0)
        {
            _logger.LogWarning("{Entity}: {Rejected} rows skipped due to invalid Id: {CustardId}. Skipped rows (as Id, CustardId) = {Rows}",
                nameof(SandEntity),
                rejected,
                nameof(SandEntity.CustardId),
                rejectedEntities
                    .Select(e => new { e.Id, e.CustardId })
                    .ToArray());

        }
        return validEntities;
    }

}
