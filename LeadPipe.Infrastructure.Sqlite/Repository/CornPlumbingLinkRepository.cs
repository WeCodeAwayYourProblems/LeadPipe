using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class CornPlumbingLinkRepository
    (
        PlumbingContext context,
        ILogger<CornPlumbingLinkRepository> logger
    ) : PlumbingContextRepository<CornPlumbingLink, CornPlumbingLinkRepository>(context, logger), IRepository<CornPlumbingLink>
{
    protected override IQueryable<CornPlumbingLink> WithIncludes(IQueryable<CornPlumbingLink> q)
    {
        return q
            .Include(q => q.CornEntity)
            .Include(q => q.PlumbingEntity);
    }

    public override async Task<Result<List<CornPlumbingLink>>> UpsertRangeAsync(
        List<CornPlumbingLink> entities, 
        CancellationToken ct = default) => await UpsertLinkRangeAsync(entities, ct);

}
