using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public class SandPlumbingLinkRepository(PlumbingContext context, ILogger<SandPlumbingLinkRepository> logger)
    : PlumbingContextRepository<SandPlumbingLink, SandPlumbingLinkRepository>(context, logger), IRepository<SandPlumbingLink>
{
    protected override IQueryable<SandPlumbingLink> WithIncludes(IQueryable<SandPlumbingLink> q)
    {
        return q
            .Include(c => c.SandEntity)
            .Include(c => c.PlumbingEntity);
    }
    
    public override async Task<Result<List<SandPlumbingLink>>> UpsertRangeAsync(
        List<SandPlumbingLink> entities, 
        CancellationToken ct = default) => await UpsertLinkRangeAsync(entities, ct);

}
