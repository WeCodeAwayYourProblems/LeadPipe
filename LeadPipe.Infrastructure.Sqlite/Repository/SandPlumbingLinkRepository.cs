using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public class SandPlumbingLinkRepository(PlumbingContext context, ILogger<SandPlumbingLinkRepository> logger)
    : PlumbingLinkContextRepository<SandPlumbingLink, SandPlumbingLinkRepository>(context, logger), IRepository<SandPlumbingLink>
{
    protected override IQueryable<SandPlumbingLink> WithIncludes(IQueryable<SandPlumbingLink> q)
    {
        return q
            .Include(c => c.SandEntity)
            .Include(c => c.PlumbingEntity);
    }

    protected override UpsertFields LinkDetails { get; } = new(
        TableName: TableNames.SandPlumbingLinksName,
        TempTable: $"temp_{TableNames.SandPlumbingLinksName}",
        Id1: nameof(SandPlumbingLink.SandId),
        Id2: nameof(SandPlumbingLink.PlumbingId),
        PhoneCol: nameof(SandPlumbingLink.MatchingPhone),
        DateCol: nameof(SandPlumbingLink.UnixMatchDate),
        EntityName: nameof(SandPlumbingLink)
        );

    protected override async Task AddLinks(List<SandPlumbingLink> links, int batchSize, CancellationToken ct)
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
                values.Add(link.SandId);
                values.Add(link.PlumbingId);
                values.Add(link.MatchingPhone);
                values.Add(link.UnixMatchDate);
            }

            string joined = $"INSERT INTO {LinkDetails.TempTable} VALUES {string.Join(",", rows)}";
            await _context.Database.ExecuteSqlRawAsync(joined, values, ct);
        }
    }

    public override async Task<Result<List<SandPlumbingLink>>> UpsertRangeAsync(
        List<SandPlumbingLink> links,
        CancellationToken ct = default) => await UpsertLinkRangeAsync(links, ct);

}
