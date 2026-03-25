using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class SandCornLinkRepository(
    PlumbingContext context,
    ILogger<SandCornLinkRepository> logger)
        : PlumbingContextLinkRepository<SandCornLink, SandCornLinkRepository>(context, logger), IRepository<SandCornLink>
{
    protected override IQueryable<SandCornLink> WithIncludes(IQueryable<SandCornLink> q)
    {
        return q
            .Include(x => x.CornEntity)
            .Include(x => x.SandEntity);
    }

    protected override UpsertFields LinkDetails { get; } =
    new(
        TableName: TableNames.SandCornLinksName,
        TempTable: $"temp_{TableNames.SandCornLinksName}",
        Id1: nameof(SandCornLink.SandId),
        Id2: nameof(SandCornLink.CornId),
        PhoneCol: nameof(SandCornLink.MatchingPhone),
        DateCol: nameof(SandCornLink.UnixMatchDate),
        EntityName: nameof(SandCornLink),
        ColumnCount: 4
        );

    protected override ParentFields Parent { get; } =
    new(
        Parent1Name: TableNames.SandEntitiesName,
        Parent1Id: nameof(SandEntity.Id),
        Parent2Name: TableNames.CornEntitiesName,
        Parent2Id: nameof(CornEntity.Id)
    );

    private static int[]? _columnIndexes;
    protected override int[] ColumnIndexes => _columnIndexes ??= [.. Enumerable.Range(0, LinkDetails.ColumnCount)];
    protected override async Task AddLinks(List<SandCornLink> links, int batchSize, CancellationToken ct)
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
                values.Add(link.SandId);
                values.Add(link.CornId);
                values.Add(link.MatchingPhone);
                values.Add(link.UnixMatchDate);
            }

            // Order here must match order above
            string joined = $"""
                INSERT INTO {LinkDetails.TempTable} (
                    {nameof(SandCornLink.SandId)},
                    {nameof(SandCornLink.CornId)},
                    {nameof(SandCornLink.MatchingPhone)},
                    {nameof(SandCornLink.UnixMatchDate)}
                )
                VALUES {string.Join(",", rows)}
                """;
            await _context.Database.ExecuteSqlRawAsync(joined, values, ct);
        }
    }

    public override async Task<Result<List<SandCornLink>>> UpsertRangeAsync(
        List<SandCornLink> links,
        CancellationToken ct = default) => await UpsertLinkRangeAsync(links, ct);

}
