using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class SandCaliperLinkRepository(PlumbingContext context, ILogger<SandCaliperLinkRepository> logger)
    : PlumbingContextRepository<SandCaliperLink, SandCaliperLinkRepository>(context, logger), IRepository<SandCaliperLink>
{
    protected override IQueryable<SandCaliperLink> WithIncludes(IQueryable<SandCaliperLink> q)
    {
        return q
            .Include(c => c.SandEntity)
            .Include(c => c.CaliperEntity);
    }

    public override async Task<Result<List<SandCaliperLink>>> UpsertRangeAsync(
        List<SandCaliperLink> entities, 
        CancellationToken ct = default) => await UpsertLinkRangeAsync(entities, ct);

}
