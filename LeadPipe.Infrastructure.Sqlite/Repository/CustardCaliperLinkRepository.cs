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
    ) : PlumbingContextRepository<CustardCaliperLink, CustardCaliperLinkRepository>(context, logger), IRepository<CustardCaliperLink>
{
    protected override IQueryable<CustardCaliperLink> WithIncludes(IQueryable<CustardCaliperLink> q)
    {
        return q
            .Include(q => q.Custard)
            .Include(q => q.Caliper);
    }

    public override async Task<Result<List<CustardCaliperLink>>> UpsertRangeAsync(
        List<CustardCaliperLink> entities, 
        CancellationToken ct = default) => await UpsertLinkRangeAsync(entities, ct);

}
