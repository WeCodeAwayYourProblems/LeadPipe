using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity;
using System.Linq.Expressions;

namespace LeadPipe.Infrastructure.Interfaces.Repository.MySql;

public interface ICornMySqlRepository
{
    Task<Result<List<CornMySqlEntity>>> FindAsync(
        Expression<Func<CornMySqlEntity, bool>> predicate);

    Task<Result<CornMySqlEntity>> GetByIdAsync(int id);
}
