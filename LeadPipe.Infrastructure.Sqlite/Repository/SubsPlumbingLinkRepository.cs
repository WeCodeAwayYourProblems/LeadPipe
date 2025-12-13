using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public class SubsPlumbingLinkRepository(PlumbingContext context) : PlumbingContextRepository<SubsPlumbingLink>(context), ISubsPlumbingLinkRepository
{
    public async Task<Result<List<SubsPlumbingLink>>> GetAllAsync(IEnumerable<PlumbingEntity> filter)
    {
        try
        {
            HashSet<long> ids = [.. filter.Select(p => p.Id)];
            List<SubsPlumbingLink> set = await _set
                .Where(e => ids.Contains(e.PlumbingId))
                .ToListAsync();
            return Result.Success(set);
        }
        catch (Exception ex) { return Result.Failure<List<SubsPlumbingLink>>(ex.Message); }
    }
}
