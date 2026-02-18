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

    protected override UpsertFields LinkDetails { get; } = new(
        TableName: TableNames.CornPlumbingLinksName,
        TempTable: $"temp_{TableNames.CornPlumbingLinksName}",
        Id1: nameof(CornPlumbingLink.CornId),
        Id2: nameof(CornPlumbingLink.PlumbingId),
        PhoneCol: nameof(CornPlumbingLink.MatchingPhone),
        DateCol: nameof(CornPlumbingLink.UnixMatchDate),
        EntityName: nameof(CornPlumbingLink)
        );

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

                int o = j * 4;
                rows.Add($"({{{o}}}, {{{o + 1}}}, {{{o + 2}}}, {{{o + 3}}})");
                values.Add(link.CornId);
                values.Add(link.PlumbingId);
                values.Add(link.MatchingPhone);
                values.Add(link.UnixMatchDate);
            }

            string joined = $"INSERT INTO {LinkDetails.TempTable} VALUES {string.Join(",", rows)}";
            await _context.Database.ExecuteSqlRawAsync(joined, values, ct);
        }
    }

    public override async Task<Result<List<CornPlumbingLink>>> UpsertRangeAsync(
        List<CornPlumbingLink> links,
        CancellationToken ct = default) => await UpsertLinkRangeAsync(links, ct);
}
