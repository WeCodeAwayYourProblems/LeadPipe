using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Database;
using LeadPipe.Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.RepositoryService;

internal class LeafRepository(PlumbingContext context)
{
    private readonly PlumbingContext _context = context;
    public async Task<Result<LeafEntity>> GetAsync(LeafEntity entity)
    {
        var result = await GetByPhoneNumberAsync(entity.PhoneNumber);
        return result;
    }

    public async Task<Result<LeafEntity>> GetByPhoneNumberAsync(long phoneNumber)
    {
        LeafEntity? found = await _context.LeafEntities.FindAsync(phoneNumber);
        if (found is null)
            return Result.Failure<LeafEntity>($"Entity with phone number {phoneNumber} was not found");
        return found;
    }

    public async Task<Result<List<LeafEntity>>> GetAllAsync()
    {
        List<LeafEntity>? result = await _context.LeafEntities.ToListAsync();
        if (result is null)
            return Result.Failure<List<LeafEntity>>("The desired repository is empty");
        return result;
    }

    public async Task<Result> AddAsync(LeafEntity entity)
    {
        await _context.LeafEntities.AddAsync(entity);
        await _context.SaveChangesAsync();
        return await GetAsync(entity);
    }

    public async Task<Result> UpdateAsync(LeafEntity entity)
    {
        // Check for existence
        LeafEntity? exists = await _context.LeafEntities
            .FirstOrDefaultAsync(e => e.PhoneNumber == entity.PhoneNumber);
        if (exists is null)
            return Result.Failure("The desired entity does not exist");

        // Update
        _context.LeafEntities.Update(entity);
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(long phoneNumber)
    {
        var entity = await _context.LeafEntities.FindAsync(phoneNumber);
        if (entity != null)
        {
            _context.LeafEntities.Remove(entity);
            await _context.SaveChangesAsync();
            var deleted = await GetAsync(entity);
            return deleted.IsSuccess ? Result.Failure("Failed to delete entity") : Result.Success();
        }
        return Result.Success();
    }
    public async Task<Result> DeleteAsync(LeafEntity entity)
    {
        return await DeleteAsync(entity.PhoneNumber);
    }
}
