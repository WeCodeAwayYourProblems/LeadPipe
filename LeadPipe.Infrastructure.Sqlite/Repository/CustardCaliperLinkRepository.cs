using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class CustardCaliperLinkRepository
    (
        PlumbingContext context,
        ILogger<CustardCaliperLinkRepository> logger
    ) : PlumbingContextLinkRepository<CustardCaliperLink, CustardCaliperLinkRepository>(context, logger), IRepository<CustardCaliperLink>
{
    protected override IQueryable<CustardCaliperLink> WithIncludes(IQueryable<CustardCaliperLink> q)
    {
        return q
            .Include(q => q.Custard)
            .Include(q => q.Caliper);
    }

    protected override UpsertFields LinkDetails { get; } =
    new(
        TableName: TableNames.CustardCaliperLinksName,
        TempTable: $"temp_{TableNames.CustardCaliperLinksName}",
        Id1: nameof(CustardCaliperLink.CustardId),
        Id2: nameof(CustardCaliperLink.CaliperId),
        PhoneCol: nameof(CustardCaliperLink.MatchingPhone),
        DateCol: nameof(CustardCaliperLink.UnixMatchDate),
        EntityName: nameof(CustardCaliperLink),
        ColumnCount: 4
        );

    protected override ParentFields Parent { get; } =
    new(
        Parent1Name: TableNames.CustardEntitiesName,
        Parent1Id: nameof(CustardEntity.Id),
        Parent2Name: TableNames.CaliperEntitiesName,
        Parent2Id: nameof(CaliperEntity.Id)
    );

    private static int[]? _columnIndexes;
    protected override int[] ColumnIndexes => _columnIndexes ??= [.. Enumerable.Range(0, LinkDetails.ColumnCount)];
    protected override async Task AddLinks(List<CustardCaliperLink> links, int batchSize, CancellationToken ct)
    {
        for (int i = 0; i < links.Count; i += batchSize)
        {
            var batch = links.GetRange(i, Math.Min(batchSize, links.Count - i));
            var values = new List<object>();
            var rows = new List<string>();

            for (int j = 0; j < batch.Count; j++)
            {
                var link = batch[j];

                int o = j * LinkDetails.ColumnCount;
                var placeholders = ColumnIndexes.Select(ci => $"{{{o + ci}}}");
                rows.Add($"({string.Join(", ", placeholders)})");

                // Order here must match order below
                values.Add(link.CustardId);
                values.Add(link.CaliperId);
                values.Add(link.MatchingPhone);
                values.Add(link.UnixMatchDate);
            }

            // Order here must match order above
            string joined = $"""
                INSERT INTO {LinkDetails.TempTable} (
                    {TempId1},
                    {TempId2},
                    {TempPhone},
                    {TempDate}
                )
                VALUES {string.Join(',', rows)}
                """;
            await _context.Database.ExecuteSqlRawAsync(joined, values, ct);
        }
    }

    public override async Task<Result<List<CustardCaliperLink>>> UpsertRangeAsync(
        List<CustardCaliperLink> links,
        CancellationToken ct = default) => await UpsertLinkRangeAsync(links, ct);

}
