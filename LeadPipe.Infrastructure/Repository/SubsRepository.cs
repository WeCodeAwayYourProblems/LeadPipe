using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Database;
using LeadPipe.Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.Repository;

internal class SubsRepository(PlumbingContext context)
{
    private readonly PlumbingContext _context = context;
    public async Task<Result<SubsEntity>> GetAsync(SubsEntity entity)
    {
        var result = await GetByIdAsync(entity.Id);
        return result;
    }

    public async Task<Result<SubsEntity>> GetByIdAsync(long id)
    {
        SubsEntity? found = await _context.SubsEntities.FindAsync(id);
        if (found is null)
            return Result.Failure<SubsEntity>($"Entity with phone number {id} was not found");
        return found;
    }

    public async Task<Result<List<SubsEntity>>> GetAllAsync()
    {
        List<SubsEntity>? result = await _context.SubsEntities.ToListAsync();
        if (result is null)
            return Result.Failure<List<SubsEntity>>("The desired repository is empty");
        return result;
    }

    public async Task<Result> AddAsync(SubsEntity entity)
    {
        await _context.SubsEntities.AddAsync(entity);
        await _context.SaveChangesAsync();
        return await GetAsync(entity);
    }

    public async Task<Result> HardUpdateAsync(SubsEntity entity)
    {
        // Check for existence
        SubsEntity? exists = await _context.SubsEntities
            .FirstOrDefaultAsync(e => e.Id == entity.Id);
        if (exists is null)
            return Result.Failure("The desired entity does not exist");

        // Update
        _context.SubsEntities.Update(entity);
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> UpdateValuesAsync(SubsEntity entity)
    {
        // Check for existence
        SubsEntity? exists = await _context.SubsEntities
            .FirstOrDefaultAsync(e => e.Id == entity.Id);
        if (exists is null)
            return Result.Failure("The desired entity does not exist");

        // Update
        _context.Entry(exists).CurrentValues.SetValues(entity);
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(long phoneNumber)
    {
        var entity = await _context.SubsEntities.FindAsync(phoneNumber);
        if (entity != null)
        {
            _context.SubsEntities.Remove(entity);
            await _context.SaveChangesAsync();
            var deleted = await GetAsync(entity);
            return deleted.IsSuccess ? Result.Failure("Failed to delete entity") : Result.Success();
        }
        return Result.Success();
    }
    public async Task<Result> DeleteAsync(SubsEntity entity)
    {
        return await DeleteAsync(entity.Id);
    }
}