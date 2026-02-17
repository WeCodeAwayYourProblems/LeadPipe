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
    ) : PlumbingContextRepository<CornCaliperLink, CornCaliperLinkRepository>(context, logger), IRepository<CornCaliperLink>
{
    protected override IQueryable<CornCaliperLink> WithIncludes(IQueryable<CornCaliperLink> q)
    {
        return q
            .Include(c => c.CornEntity)
            .Include(c => c.CaliperEntity);
    }

    public override async Task<Result<List<CornCaliperLink>>> UpsertRangeAsync(
        List<CornCaliperLink> entities,
        CancellationToken ct = default) => await UpsertLinkRangeAsync(entities, ct);
}