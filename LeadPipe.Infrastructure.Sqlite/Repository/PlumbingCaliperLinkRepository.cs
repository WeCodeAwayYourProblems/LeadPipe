using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class PlumbingCaliperLinkRepository
    (
        PlumbingContext context,
        ILogger<PlumbingCaliperLinkRepository> logger
    ) : PlumbingContextRepository<PlumbingCaliperLink, PlumbingCaliperLinkRepository>(context, logger), IRepository<PlumbingCaliperLink>
{
    protected override IQueryable<PlumbingCaliperLink> WithIncludes(IQueryable<PlumbingCaliperLink> q)
    {
        return q
            .Include(c => c.PlumbingEntity)
            .Include(c => c.CaliperEntity);
    }

    public override async Task<Result<List<PlumbingCaliperLink>>> UpsertRangeAsync(
        List<PlumbingCaliperLink> entities, 
        CancellationToken ct = default) => await UpsertLinkRangeAsync(entities, ct);

}
