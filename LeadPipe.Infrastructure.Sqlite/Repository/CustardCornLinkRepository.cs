using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Database.Configuration;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Repository;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class CustardCornLinkRepository
    (
        PlumbingContext context,
        ILogger<CustardCornLinkRepository> logger
    ) : PlumbingContextLinkRepository<CustardCornLink, CustardCornLinkRepository>(context, logger), IRepository<CustardCornLink>
{
    protected override IQueryable<CustardCornLink> WithIncludes(IQueryable<CustardCornLink> q)
    {
        return q
            .Include(q => q.Custard)
            .Include(q => q.Corn);
    }

    protected override UpsertFields LinkDetails { get; } =
    new(
        TableName: TableNames.CustardCornLinksName,
        TempTable: $"temp_{TableNames.CustardCornLinksName}",
        Id1: nameof(CustardCornLink.CustardId),
        Id2: nameof(CustardCornLink.CornId),
        PhoneCol: nameof(CustardCornLink.MatchingPhone),
        DateCol: nameof(CustardCornLink.UnixMatchDate),
        EntityName: nameof(CustardCornLink),
        ColumnCount: 4
    );

    protected override ParentFields Parent { get; } =
        new(
            Parent1Name: TableNames.CustardEntitiesName,
            Parent1Id: nameof(CustardEntity.Id),
            Parent2Name: TableNames.CornEntitiesName,
            Parent2Id: nameof(CornEntity.Id)
    );

    private static int[]? _columnIndexes;
    protected override int[] ColumnIndexes => _columnIndexes ??= [.. Enumerable.Range(0, LinkDetails.ColumnCount)];
    protected override async Task AddLinks(List<CustardCornLink> links, int batchSize, CancellationToken ct)
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
                values.Add(link.CornId);
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

    public override async Task<Result<List<CustardCornLink>>> UpsertRangeAsync(
        List<CustardCornLink> links,
        CancellationToken ct = default) => await UpsertLinkRangeAsync(links, ct);

}
