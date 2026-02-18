using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class PlumbingCaliperLinkRepository
    (
        PlumbingContext context,
        ILogger<PlumbingCaliperLinkRepository> logger
    ) : PlumbingLinkContextRepository<PlumbingCaliperLink, PlumbingCaliperLinkRepository>(context, logger), IRepository<PlumbingCaliperLink>
{
    protected override IQueryable<PlumbingCaliperLink> WithIncludes(IQueryable<PlumbingCaliperLink> q)
    {
        return q
            .Include(c => c.PlumbingEntity)
            .Include(c => c.CaliperEntity);
    }

    protected override UpsertFields LinkDetails { get; } = new(
        TableName: TableNames.PlumbingCaliperLinksName,
        TempTable: $"temp_{TableNames.PlumbingCaliperLinksName}",
        Id1: nameof(PlumbingCaliperLink.PlumbingId),
        Id2: nameof(PlumbingCaliperLink.CaliperId),
        PhoneCol: nameof(PlumbingCaliperLink.MatchingPhone),
        DateCol: nameof(PlumbingCaliperLink.UnixMatchDate),
        EntityName: nameof(PlumbingCaliperLink)
        );

    protected override async Task AddLinks(List<PlumbingCaliperLink> links, int batchSize, CancellationToken ct)
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
                values.Add(link.PlumbingId);
                values.Add(link.CaliperId);
                values.Add(link.MatchingPhone);
                values.Add(link.UnixMatchDate);
            }

            string joined = $"INSERT INTO {LinkDetails.TempTable} VALUES {string.Join(",", rows)}";
            await _context.Database.ExecuteSqlRawAsync(joined, values, ct);
        }
    }

    public override async Task<Result<List<PlumbingCaliperLink>>> UpsertRangeAsync(
        List<PlumbingCaliperLink> links,
        CancellationToken ct = default) => await UpsertLinkRangeAsync(links, ct);

}
