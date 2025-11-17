using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Database;
using LeadPipe.Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.Repository;

internal class SubsPlumbLinkRepository(PlumbingContext context) : ISubsPlumbLinkRepository
{
    private readonly PlumbingContext _context = context;
    public async Task<Result<SubsPlumbingLink>> GetAsync(SubsPlumbingLink entity)
    {
        var result = await GetByIdAsync(entity.SubsId, entity.PlumbingId);
        return result;
    }

    public async Task<Result<SubsPlumbingLink>> GetByIdAsync(long subsId, long plumbId)
    {
        SubsPlumbingLink? found = await _context.SubsPlumbingLinks.FindAsync(subsId, plumbId);
        return found is null
            ? Result.Failure<SubsPlumbingLink>($"Entity with compound id was not found\nSub Id: {subsId}\nPlumb Id: {plumbId}")
            : Result.Success(found);
    }

    public async Task<Result<List<SubsPlumbingLink>>> GetAllAsync()
    {
        List<SubsPlumbingLink>? result = await _context.SubsPlumbingLinks.ToListAsync();
        return result is null
            ? Result.Failure<List<SubsPlumbingLink>>("The desired repository is empty")
            : Result.Success(result);
    }

    public async Task<Result> AddAsync(SubsPlumbingLink entity)
    {
        await _context.SubsPlumbingLinks.AddAsync(entity);
        await _context.SaveChangesAsync();
        return await GetAsync(entity);
    }

    public async Task<Result> AddRangeAsync(List<SubsPlumbingLink> entities)
    {
        if (entities == null || entities.Count == 0)
            return Result.Failure("No link entities provided.");

        await _context.SubsPlumbingLinks.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        var allLinksExist = entities.All(l =>
            _context.SubsPlumbingLinks.Any(dbLink =>
                dbLink.SubsId == l.SubsId && dbLink.PlumbingId == l.PlumbingId));

        return allLinksExist
            ? Result.Success()
            : Result.Failure("Not all link entities were saved successfully.");
    }


    public async Task<Result> HardUpdateAsync(SubsPlumbingLink entity)
    {
        // Check for existence
        SubsPlumbingLink? exists = await _context.SubsPlumbingLinks
            .FirstOrDefaultAsync(e => e.SubsId == entity.SubsId && e.PlumbingId == entity.PlumbingId);
        if (exists is null)
            return Result.Failure("The desired entity does not exist");

        // Update
        _context.SubsPlumbingLinks.Update(entity);
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> UpdateValuesAsync(SubsPlumbingLink entity)
    {
        // Check for existence
        SubsPlumbingLink? exists = await _context.SubsPlumbingLinks
            .FirstOrDefaultAsync(e => e.SubsId == entity.SubsId && e.PlumbingId == entity.PlumbingId);
        if (exists is null)
            return Result.Failure("The desired entity does not exist");

        // Update
        _context.Entry(exists).CurrentValues.SetValues(entity);
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(long subsId, long plumbId)
    {
        var entity = await _context.SubsPlumbingLinks.FindAsync(subsId, plumbId);
        if (entity != null)
        {
            _context.SubsPlumbingLinks.Remove(entity);
            await _context.SaveChangesAsync();
            var deleted = await GetAsync(entity);
            return deleted.IsSuccess ? Result.Failure("Failed to delete entity") : Result.Success();
        }
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(SubsPlumbingLink entity)
    {
        return await DeleteAsync(entity.SubsId, entity.PlumbingId);
    }
}
