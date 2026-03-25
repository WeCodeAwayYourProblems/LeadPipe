using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class CornPlumbingLinkRepository
    (
        PlumbingContext context,
        ILogger<CornPlumbingLinkRepository> logger
    ) : PlumbingContextLinkRepository<CornPlumbingLink, CornPlumbingLinkRepository>(context, logger), IRepository<CornPlumbingLink>
{
    protected override IQueryable<CornPlumbingLink> WithIncludes(IQueryable<CornPlumbingLink> q)
    {
        return q
            .Include(q => q.CornEntity)
            .Include(q => q.PlumbingEntity);
    }

    protected override UpsertFields LinkDetails { get; } =
    new(
        TableName: TableNames.CornPlumbingLinksName,
        TempTable: $"temp_{TableNames.CornPlumbingLinksName}",
        Id1: nameof(CornPlumbingLink.CornId),
        Id2: nameof(CornPlumbingLink.PlumbingId),
        PhoneCol: nameof(CornPlumbingLink.MatchingPhone),
        DateCol: nameof(CornPlumbingLink.UnixMatchDate),
        EntityName: nameof(CornPlumbingLink),
        ColumnCount: 4
        );

    protected override ParentFields Parent { get; } =
    new(
        Parent1Name: TableNames.CornEntitiesName,
        Parent1Id: nameof(CornEntity.Id),
        Parent2Name: TableNames.PlumbingEntitiesName,
        Parent2Id: nameof(PlumbingEntity.Id)
    );

    private static int[]? _columnIndexes;
    protected override int[] ColumnIndexes => _columnIndexes ??= [.. Enumerable.Range(0, LinkDetails.ColumnCount)];
    protected override async Task AddLinks(List<CornPlumbingLink> links, int batchSize, CancellationToken ct)
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
                var placeholders = ColumnIndexes.Select(columnIndex => $"{{{o + columnIndex}}}");
                rows.Add($"({string.Join(", ", placeholders)})");

                // Order here must match order below
                values.Add(link.CornId);
                values.Add(link.PlumbingId);
                values.Add(link.MatchingPhone);
                values.Add(link.UnixMatchDate);
            }

            // Order here must match order above
            string joined = $"""
                INSERT INTO {LinkDetails.TempTable} (
                    {nameof(CornPlumbingLink.CornId)},
                    {nameof(CornPlumbingLink.PlumbingId)},
                    {nameof(CornPlumbingLink.MatchingPhone)},
                    {nameof(CornPlumbingLink.UnixMatchDate)}
                ) 
                VALUES {string.Join(",", rows)}
                """;
            await _context.Database.ExecuteSqlRawAsync(joined, values, ct);
        }
    }

    public override async Task<Result<List<CornPlumbingLink>>> UpsertRangeAsync(
        List<CornPlumbingLink> links,
        CancellationToken ct = default) => await UpsertLinkRangeAsync(links, ct);

}
