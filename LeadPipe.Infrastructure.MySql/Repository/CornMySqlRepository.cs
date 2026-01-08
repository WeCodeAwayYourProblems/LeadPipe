using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces.Repository.MySql;
using LeadPipe.Infrastructure.MySql.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LeadPipe.Infrastructure.MySql.Repository;

public class CornMySqlRepository(MySqlSchema3Context context)
    : ICornMySqlRepository
{
    private readonly DbSet<CornMySqlEntity> _set = context.Set<CornMySqlEntity>();

    public async Task<Result<List<CornMySqlEntity>>> FindAsync(
        Expression<Func<CornMySqlEntity, bool>> predicate)
    {
        try
        {
            List<CornMySqlEntity> list = await _set
                .AsNoTracking()
                .Where(predicate)
                .ToListAsync();

            return Result.Success(list);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<CornMySqlEntity>>(ex.Message);
        }
    }

    public async Task<Result<CornMySqlEntity>> GetByIdAsync(int id)
    {
        CornMySqlEntity? found = await _set
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.id == id);

        return found is null
            ? Result.Failure<CornMySqlEntity>(
                $"{nameof(CornMySqlEntity)} with id {id} was not found")
            : Result.Success(found);
    }
}
