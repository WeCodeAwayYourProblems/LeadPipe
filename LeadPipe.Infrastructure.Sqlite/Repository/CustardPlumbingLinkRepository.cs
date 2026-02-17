using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class CustardPlumbingLinkRepository
    (
        PlumbingContext context,
        ILogger<CustardPlumbingLinkRepository> logger
    ) : PlumbingContextRepository<CustardPlumbingLink, CustardPlumbingLinkRepository>(context, logger), IRepository<CustardPlumbingLink>
{
    protected override IQueryable<CustardPlumbingLink> WithIncludes(IQueryable<CustardPlumbingLink> q)
    {
        return q
            .Include(q => q.Custard)
            .Include(q => q.Plumbing);
    }

    public override async Task<Result<List<CustardPlumbingLink>>> UpsertRangeAsync(
        List<CustardPlumbingLink> entities, 
        CancellationToken ct = default) => await UpsertLinkRangeAsync(entities, ct);

}
