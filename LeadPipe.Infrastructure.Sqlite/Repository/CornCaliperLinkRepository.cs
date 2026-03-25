using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class CornCaliperLinkRepository
    (
        PlumbingContext context,
        ILogger<CornCaliperLinkRepository> logger
    ) : PlumbingContextLinkRepository<CornCaliperLink, CornCaliperLinkRepository>(context, logger), IRepository<CornCaliperLink>
{
    protected override IQueryable<CornCaliperLink> WithIncludes(IQueryable<CornCaliperLink> q)
    {
        return q
            .Include(c => c.CornEntity)
            .Include(c => c.CaliperEntity);
    }

    protected override UpsertFields LinkDetails { get; } =
    new(
        TableName: TableNames.CornCaliperLinksName,
        TempTable: $"temp_{TableNames.CornCaliperLinksName}",
        Id1: nameof(CornCaliperLink.CornId),
        Id2: nameof(CornCaliperLink.CaliperId),
        PhoneCol: nameof(CornCaliperLink.MatchingPhone),
        DateCol: nameof(CornCaliperLink.UnixMatchDate),
        EntityName: nameof(CornCaliperLink),
        ColumnCount: 4
        );
    
    protected override ParentFields Parent { get; } = 
    new(
        Parent1Name: TableNames.CornEntitiesName,
        Parent1Id: nameof(CornEntity.Id),
        Parent2Name: TableNames.CaliperEntitiesName,
        Parent2Id: nameof(CaliperEntity.Id)
    );

    private static int[]? _columnIndexes;
    protected override int[] ColumnIndexes => _columnIndexes ??= [.. Enumerable.Range(0, LinkDetails.ColumnCount)];
    protected override async Task AddLinks(List<CornCaliperLink> links, int batchSize, CancellationToken ct)
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
                values.Add(link.CornId); // id1
                values.Add(link.CaliperId); // id2
                values.Add(link.MatchingPhone); // phone
                values.Add(link.UnixMatchDate); // matchdate
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

    public override async Task<Result<List<CornCaliperLink>>> UpsertRangeAsync(
        List<CornCaliperLink> links,
        CancellationToken ct = default) => await UpsertLinkRangeAsync(links, ct);

}