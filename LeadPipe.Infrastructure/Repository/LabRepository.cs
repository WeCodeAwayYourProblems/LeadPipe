using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.DbContext;
using LeadPipe.Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.RepositoryService;

internal class LabRepository(PlumbingContext context)
{
    private readonly PlumbingContext _context = context;
    public async Task<Result<LabEntity>> GetAsync(LabEntity entity)
    {
        var result = await GetByPhoneNumberAsync(entity.PhoneNumber);
        return result;
    }

    public async Task<Result<LabEntity>> GetByPhoneNumberAsync(long phoneNumber)
    {
        LabEntity? found = await _context.LabEntities.FindAsync(phoneNumber);
        if (found is null)
            return Result.Failure<LabEntity>($"Entity with phone number {phoneNumber} was not found");
        return found;
    }

    public async Task<Result<List<LabEntity>>> GetAllAsync()
    {
        List<LabEntity>? result = await _context.LabEntities.ToListAsync();
        if (result is null)
            return Result.Failure<List<LabEntity>>("The desired repository is empty");
        return result;
    }

    public async Task<Result> AddAsync(LabEntity entity)
    {
        await _context.LabEntities.AddAsync(entity);
        await _context.SaveChangesAsync();
        return await GetAsync(entity);
    }

    public async Task<Result> UpdateAsync(LabEntity entity)
    {
        // Check for existence
        LabEntity? exists = await _context.LabEntities
            .FirstOrDefaultAsync(e => e.PhoneNumber == entity.PhoneNumber);
        if (exists is null)
            return Result.Failure("The desired entity does not exist");

        // Update
        _context.LabEntities.Update(entity);
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(long phoneNumber)
    {
        var entity = await _context.LabEntities.FindAsync(phoneNumber);
        if (entity != null)
        {
            _context.LabEntities.Remove(entity);
            await _context.SaveChangesAsync();
            var deleted = await GetAsync(entity);
            return deleted.IsSuccess ? Result.Failure("Failed to delete entity") : Result.Success();
        }
        return Result.Success();
    }
    public async Task<Result> DeleteAsync(LabEntity entity)
    {
        return await DeleteAsync(entity.PhoneNumber);
    }
}
