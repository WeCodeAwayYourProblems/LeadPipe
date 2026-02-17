using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class SandCornLinkRepository(
    PlumbingContext context,
    ILogger<SandCornLinkRepository> logger)
        : PlumbingContextRepository<SandCornLink, SandCornLinkRepository>(context, logger), IRepository<SandCornLink>
{
    protected override IQueryable<SandCornLink> WithIncludes(IQueryable<SandCornLink> q)
    {
        return q.Include(x => x.CornEntity)
                .Include(x => x.SandEntity);
    }

    public override async Task<Result<List<SandCornLink>>> UpsertRangeAsync(
        List<SandCornLink> entities, 
        CancellationToken ct = default) => await UpsertLinkRangeAsync(entities, ct);

}
