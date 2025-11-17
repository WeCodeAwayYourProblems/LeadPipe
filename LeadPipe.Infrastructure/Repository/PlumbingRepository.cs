using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Database;
using LeadPipe.Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.Repository;

internal class PlumbingRepository(PlumbingContext context) : IPlumbingRepository
{
    private readonly PlumbingContext _context = context;
    public async Task<Result<PlumbingEntity>> GetAsync(PlumbingEntity entity)
    {
        Result<PlumbingEntity> result = await GetByIdAsync(entity.Id);
        return result;
    }

    public async Task<Result<PlumbingEntity>> GetByIdAsync(long id)
    {
        PlumbingEntity? found = await _context.PlumbingEntities.FindAsync(id);
        if (found is null)
            return Result.Failure<PlumbingEntity>($"Entity with phone number {id} was not found");
        return found;
    }

    public async Task<Result<List<PlumbingEntity>>> GetAllAsync()
    {
        List<PlumbingEntity>? result = await _context.PlumbingEntities.ToListAsync();
        if (result is null)
            return Result.Failure<List<PlumbingEntity>>("The desired repository is empty");
        return result;
    }

    public async Task<Result> AddAsync(PlumbingEntity entity)
    {
        await _context.PlumbingEntities.AddAsync(entity);
        await _context.SaveChangesAsync();
        return await GetAsync(entity);
    }

    public async Task<Result> AddRangeAsync(List<PlumbingEntity> entities)
    {
        if (entities is null || entities.Count == 0)
            return Result.Failure("No plumbing entities provided.");

        await _context.PlumbingEntities.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        // Verify all entities exist
        List<long> ids = entities.Select(e => e.Id).ToList();
        List<PlumbingEntity> savedEntities = await _context.PlumbingEntities
            .Where(e => ids.Contains(e.Id))
            .ToListAsync();

        return savedEntities.Count == entities.Count
            ? Result.Success()
            : Result.Failure("Not all plumbing entities were saved successfully.");
    }


    public async Task<Result> HardUpdateAsync(PlumbingEntity entity)
    {
        // Check for existence
        PlumbingEntity? exists = await _context.PlumbingEntities
            .FirstOrDefaultAsync(e => e.Id == entity.Id);
        if (exists is null)
            return Result.Failure("The desired entity does not exist");

        // Update
        _context.PlumbingEntities.Update(entity);
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> UpdateValuesAsync(PlumbingEntity entity)
    {
        // Check for existence
        PlumbingEntity? exists = await _context.PlumbingEntities
            .FirstOrDefaultAsync(e => e.Id == entity.Id);
        if (exists is null)
            return Result.Failure("The desired entity does not exist");

        // Update
        _context.Entry(exists).CurrentValues.SetValues(entity);
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(long id)
    {
        PlumbingEntity? entity = await _context.PlumbingEntities.FindAsync(id);
        if (entity is not null)
        {
            _context.PlumbingEntities.Remove(entity);
            await _context.SaveChangesAsync();
            Result<PlumbingEntity> deleted = await GetAsync(entity);
            return deleted.IsSuccess ? Result.Failure("Failed to delete entity") : Result.Success();
        }
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(PlumbingEntity entity)
    {
        return await DeleteAsync(entity.Id);
    }
}