
using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.DbContext;
using LeadPipe.Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.RepositoryService;

internal class YellerRepository(PlumbingContext context)
{
    private readonly PlumbingContext _context = context;
    public async Task<Result<YellerEntity>> GetAsync(YellerEntity entity)
    {
        var result = await GetByPhoneNumberAsync(entity.PhoneNumber);
        return result;
    }

    public async Task<Result<YellerEntity>> GetByPhoneNumberAsync(long phoneNumber)
    {
        YellerEntity? found = await _context.YellerEntities.FindAsync(phoneNumber);
        if (found is null)
            return Result.Failure<YellerEntity>($"Entity with phone number {phoneNumber} was not found");
        return found;
    }

    public async Task<Result<List<YellerEntity>>> GetAllAsync()
    {
        List<YellerEntity>? result = await _context.YellerEntities.ToListAsync();
        if (result is null)
            return Result.Failure<List<YellerEntity>>("The desired repository is empty");
        return result;
    }

    public async Task<Result> AddAsync(YellerEntity entity)
    {
        await _context.YellerEntities.AddAsync(entity);
        await _context.SaveChangesAsync();
        return await GetAsync(entity);
    }

    public async Task<Result> UpdateAsync(YellerEntity entity)
    {
        // Check for existence
        YellerEntity? exists = await _context.YellerEntities
            .FirstOrDefaultAsync(e => e.PhoneNumber == entity.PhoneNumber);
        if (exists is null)
            return Result.Failure("The desired entity does not exist");

        // Update
        _context.YellerEntities.Update(entity);
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(long phoneNumber)
    {
        var entity = await _context.YellerEntities.FindAsync(phoneNumber);
        if (entity != null)
        {
            _context.YellerEntities.Remove(entity);
            await _context.SaveChangesAsync();
            var deleted = await GetAsync(entity);
            return deleted.IsSuccess ? Result.Failure("Failed to delete entity") : Result.Success();
        }
        return Result.Success();
    }
    public async Task<Result> DeleteAsync(YellerEntity entity)
    {
        return await DeleteAsync(entity.PhoneNumber);
    }
}