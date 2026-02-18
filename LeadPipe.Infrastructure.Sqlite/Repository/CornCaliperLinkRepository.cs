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
    
    protected override UpsertFields LinkDetails { get; } = new(
        TableName: TableNames.CornCaliperLinksName,
        TempTable: $"temp_{TableNames.CornCaliperLinksName}",
        Id1: nameof(CornCaliperLink.CornId),
        Id2: nameof(CornCaliperLink.CaliperId),
        PhoneCol: nameof(CornCaliperLink.MatchingPhone),
        DateCol: nameof(CornCaliperLink.UnixMatchDate),
        EntityName: nameof(CornCaliperLink)
        );

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

                int o = j * 4;
                rows.Add($"({{{o}}}, {{{o + 1}}}, {{{o + 2}}}, {{{o + 3}}})");
                values.Add(link.CornId);
                values.Add(link.CaliperId);
                values.Add(link.MatchingPhone);
                values.Add(link.UnixMatchDate);
            }

            string joined = $"INSERT INTO {LinkDetails.TempTable} VALUES {string.Join(",", rows)}";
            await _context.Database.ExecuteSqlRawAsync(joined, values, ct);
        }
    }

    public override async Task<Result<List<CornCaliperLink>>> UpsertRangeAsync(
        List<CornCaliperLink> links,
        CancellationToken ct = default) => await UpsertLinkRangeAsync(links, ct);
}