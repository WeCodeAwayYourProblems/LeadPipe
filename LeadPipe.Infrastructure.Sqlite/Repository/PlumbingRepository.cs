using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public class PlumbingRepository(
    PlumbingContext context, 
    ILogger<PlumbingRepository> logger
    ) : PlumbingContextRepository<PlumbingEntity>(context), IPlumbingRepository
{
    private readonly ILogger<PlumbingRepository> _logger = logger;
    public async Task<Result<List<PlumbingEntity>>> GetAllAsync(Source source)
    {
        try
        {
            List<PlumbingEntity> plumbing = await _set
                .Where(p => p.Source == source)
                .ToListAsync();
            return Result.Success(plumbing);
        }
        catch (Exception ex) { return Result.Failure<List<PlumbingEntity>>(ex.Message); }
    }
    public override async Task<Result<List<PlumbingEntity>>> AddRangeAsync(List<PlumbingEntity> entities)
    {
        if (entities == null || entities.Count == 0)
            return Result.Failure<List<PlumbingEntity>>("No entities provided.");

        try
        {
            // Extract numbers
            HashSet<long> phoneNumbers = [.. entities.Select(e => e.PhoneNumber)];

            // Query
            var existing = await _set
                .Where(p => phoneNumbers.Contains(p.PhoneNumber))
                .Select(p => new { p.PhoneNumber, p.Source })
                .ToListAsync();

            // Now finish the composite match in memory
            HashSet<(long PhoneNumber, Source Source)> existingSet = [.. existing.Select(x => (x.PhoneNumber, x.Source))];

            List<PlumbingEntity> toInsert = [.. entities.Where(e => !existingSet.Contains((e.PhoneNumber, e.Source)))];

            if (toInsert.Count == 0)
                return Result.Success(new List<PlumbingEntity>());

            await _set.AddRangeAsync(toInsert);
            await _context.SaveChangesAsync();

            _logger.LogDebug(
                "Plumbing bulk insert: {Inserted}/{Total}",
                toInsert.Count,
                entities.Count);

            return Result.Success(toInsert);
        }
        catch (Exception ex) { return Result.Failure<List<PlumbingEntity>>($"Failed to save Plumbing entities: {ex.Message}"); }
    }

}
