using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public class PlumbingRepository(PlumbingContext context) : PlumbingContextRepository<PlumbingEntity>(context), IPlumbingRepository
{
    public override async Task<Result<List<PlumbingEntity>>> AddRangeAsync(List<PlumbingEntity> entities)
    {
        if (entities == null || entities.Count == 0)
            return Result.Failure<List<PlumbingEntity>>("No entities provided.");

        try
        {
            var combos = entities.Select(e => new { e.PhoneNumber, e.Source });

            // Pull only rows that match any combo
            var existing = await _set
                .Where(p => combos.Contains(new { p.PhoneNumber, p.Source }))
                .Select(p => new { p.PhoneNumber, p.Source })
                .ToListAsync();

            // Use a HashSet for faster duplicate filtering
            var existingSet = new HashSet<(long PhoneNumber, Source Source)>(
                existing.Select(x => (x.PhoneNumber, x.Source))
            );

            var toInsert = entities
                .Where(e => !existingSet.Contains((e.PhoneNumber, e.Source)))
                .ToList();

            if (toInsert.Count == 0)
                return Result.Success(new List<PlumbingEntity>());

            await _set.AddRangeAsync(toInsert);
            await _context.SaveChangesAsync();

            return Result.Success(toInsert);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<PlumbingEntity>>($"Failed to save Plumbing entities: {ex.Message}");
        }
    }

}
