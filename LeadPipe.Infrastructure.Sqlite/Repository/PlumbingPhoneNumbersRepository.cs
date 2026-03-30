using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public class PlumbingPhoneNumbersRepository
    (
        PlumbingContext context,
        ILogger<PlumbingPhoneNumbersRepository> logger
    ) : PlumbingContextEntityRepository<PlumbingPhoneNumber, PlumbingPhoneNumbersRepository>(context, logger)
{
    protected override IQueryable<PlumbingPhoneNumber> WithIncludes(IQueryable<PlumbingPhoneNumber> q) 
        => q
            .Include(c => c.Plumbing);

    protected override UpsertFields EntityDetails { get; } =
        new(
            TableName: TableNames.PlumbingPhoneNumbersName,
            TempTable: $"temp_{TableNames.PlumbingPhoneNumbersName}",
            EntityName: nameof(PlumbingPhoneNumber),
            ColumnCount: 2
            );

    protected override string CreateTempTable => $"""
        CREATE TEMP TABLE IF NOT EXISTS {EntityDetails.TempTable} (
            {nameof(PlumbingPhoneNumber.PhoneNumber)} INTEGER NOT NULL,
            {nameof(PlumbingPhoneNumber.PlumbingId)} INTEGER NOT NULL,
            PRIMARY KEY (
                {nameof(PlumbingPhoneNumber.PhoneNumber)},
                {nameof(PlumbingPhoneNumber.PlumbingId)}
            )
        ) WITHOUT ROWID;
        DELETE FROM {EntityDetails.TempTable};
    """;

    // Should we throw or should we just let it slide?
    protected override string UpdateSql => throw new InvalidOperationException($"Table {EntityDetails.TableName} is not updatable");

    protected override string InsertSql => $"""
        INSERT INTO {EntityDetails.TableName} (
            {nameof(PlumbingPhoneNumber.PhoneNumber)},
            {nameof(PlumbingPhoneNumber.PlumbingId)}
        )
        SELECT 
            temp.{nameof(PlumbingPhoneNumber.PhoneNumber)},
            temp.{nameof(PlumbingPhoneNumber.PlumbingId)}
        FROM {EntityDetails.TempTable} temp
        WHERE NOT EXISTS (
            SELECT 1
            FROM {TableNames.PlumbingPhoneNumbersName} t
            WHERE t.{nameof(PlumbingPhoneNumber.PhoneNumber)} = temp.{nameof(PlumbingPhoneNumber.PhoneNumber)}
                AND t.{nameof(PlumbingPhoneNumber.PlumbingId)} = temp.{nameof(PlumbingPhoneNumber.PlumbingId)}
        );
    """;

    protected override bool IsUpdatable => false;

    private static int[]? _columnIndexes;
    protected override int[] ColumnIndexes => _columnIndexes ??= [.. Enumerable.Range(0, EntityDetails.ColumnCount)];
    protected override void InsertBatch(List<PlumbingPhoneNumber> batch)
    {
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
            values.Add(e.PlumbingId);
        }

        // Order here must match order above
        string joined = $"""
            INSERT INTO {EntityDetails.TempTable} (
                {nameof(PlumbingPhoneNumber.PhoneNumber)},
                {nameof(PlumbingPhoneNumber.PlumbingId)}
            )
            VALUES {string.Join(',', rows)}
         """;

        _context.Database.ExecuteSqlRaw(joined, [.. values]);
    }

    public override async Task<Result<List<PlumbingPhoneNumber>>> UpsertRangeAsync(
        List<PlumbingPhoneNumber> entities,
        CancellationToken ct = default) => await UpsertEntityRangeAsync(entities, ct);

}