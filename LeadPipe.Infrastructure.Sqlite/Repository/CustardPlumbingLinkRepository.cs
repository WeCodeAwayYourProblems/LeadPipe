using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Database.Configuration;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class CustardPlumbingLinkRepository
    (
        PlumbingContext context,
        ILogger<CustardPlumbingLinkRepository> logger
    ) : PlumbingContextLinkRepository<CustardPlumbingLink, CustardPlumbingLinkRepository>(context, logger), IRepository<CustardPlumbingLink>
{
    protected override IQueryable<CustardPlumbingLink> WithIncludes(IQueryable<CustardPlumbingLink> q)
    {
        return q
            .Include(q => q.Custard)
            .Include(q => q.Plumbing);
    }

    protected override UpsertFields LinkDetails { get; } =
    new(
        TableName: TableNames.CustardPlumbingLinksName,
        TempTable: $"temp_{TableNames.CustardPlumbingLinksName}",
        Id1: nameof(CustardPlumbingLink.CustardId),
        Id2: nameof(CustardPlumbingLink.PlumbingId),
        PhoneCol: nameof(CustardPlumbingLink.MatchingPhone),
        DateCol: nameof(CustardPlumbingLink.UnixMatchDate),
        EntityName: nameof(CustardPlumbingLink),
        ColumnCount: 4
        );

    protected override ParentFields Parent { get; } =
    new(
        Parent1Name: TableNames.CustardEntitiesName,
        Parent1Id: nameof(CustardEntity.Id),
        Parent2Name: TableNames.PlumbingEntitiesName,
        Parent2Id: nameof(PlumbingEntity.Id)
    );

    private static int[]? _columnIndexes;
    protected override int[] ColumnIndexes => _columnIndexes ??= [.. Enumerable.Range(0, LinkDetails.ColumnCount)];
    protected override async Task AddLinks(List<CustardPlumbingLink> links, int batchSize, CancellationToken ct)
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
                values.Add(link.PlumbingId);
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

    public override async Task<Result<List<CustardPlumbingLink>>> UpsertRangeAsync(
        List<CustardPlumbingLink> links,
        CancellationToken ct = default) => await UpsertLinkRangeAsync(links, ct);

}
