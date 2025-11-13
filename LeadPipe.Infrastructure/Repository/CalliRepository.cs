using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Database;
using LeadPipe.Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.RepositoryService;

internal class CalliRepository(PlumbingContext context)
{
    private readonly PlumbingContext _context = context;
    public async Task<Result<CalliEntity>> GetAsync(CalliEntity entity)
    {
        var result = await GetByPhoneNumberAsync(entity.PhoneNumber);
        return result;
    }

    public async Task<Result<CalliEntity>> GetByPhoneNumberAsync(long phoneNumber)
    {
        CalliEntity? found = await _context.CalliEntities.FindAsync(phoneNumber);
        if (found is null)
            return Result.Failure<CalliEntity>($"Entity with phone number {phoneNumber} was not found");
        return found;
    }

    public async Task<Result<List<CalliEntity>>> GetAllAsync()
    {
        List<CalliEntity>? result = await _context.CalliEntities.ToListAsync();
        if (result is null)
            return Result.Failure<List<CalliEntity>>("The desired repository is empty");
        return result;
    }

    public async Task<Result> AddAsync(CalliEntity entity)
    {
        await _context.CalliEntities.AddAsync(entity);
        await _context.SaveChangesAsync();
        return await GetAsync(entity);
    }

    public async Task<Result> UpdateAsync(CalliEntity entity)
    {
        // Check for existence
        CalliEntity? exists = await _context.CalliEntities
            .FirstOrDefaultAsync(e => e.PhoneNumber == entity.PhoneNumber);
        if (exists is null)
            return Result.Failure("The desired entity does not exist");

        // Update
        _context.CalliEntities.Update(entity);
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(long phoneNumber)
    {
        var entity = await _context.CalliEntities.FindAsync(phoneNumber);
        if (entity != null)
        {
            _context.CalliEntities.Remove(entity);
            await _context.SaveChangesAsync();
            var deleted = await GetAsync(entity);
            return deleted.IsSuccess ? Result.Failure("Failed to delete entity") : Result.Success();
        }
        return Result.Success();
    }
    public async Task<Result> DeleteAsync(CalliEntity entity)
    {
        return await DeleteAsync(entity.PhoneNumber);
    }
}