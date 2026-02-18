using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

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

    protected override UpsertFields LinkDetails { get; } = new(
        TableName: TableNames.CustardCaliperLinksName,
        TempTable: $"temp_{TableNames.CustardCaliperLinksName}",
        Id1: nameof(CustardCaliperLink.CustardId),
        Id2: nameof(CustardCaliperLink.CaliperId),
        PhoneCol: nameof(CustardCaliperLink.MatchingPhone),
        DateCol: nameof(CustardCaliperLink.UnixMatchDate),
        EntityName: nameof(CustardCaliperLink)
        );

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

                int o = j * 4;
                rows.Add($"({{{o}}}, {{{o + 1}}}, {{{o + 2}}}, {{{o + 3}}})");
                values.Add(link.CustardId);
                values.Add(link.CaliperId);
                values.Add(link.MatchingPhone);
                values.Add(link.UnixMatchDate);
            }

            string joined = $"INSERT INTO {LinkDetails.TempTable} VALUES {string.Join(",", rows)}";
            await _context.Database.ExecuteSqlRawAsync(joined, values, ct);
        }
    }

    public override async Task<Result<List<CustardCaliperLink>>> UpsertRangeAsync(
        List<CustardCaliperLink> links,
        CancellationToken ct = default) => await UpsertLinkRangeAsync(links, ct);

}
