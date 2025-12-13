using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using System.Linq.Expressions;

namespace LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

public interface IRepository<T> where T : class, IEntity
{
    Task<Result<List<T>>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<Result<T>> AddAsync(T entity);
    Task<Result<List<T>>> AddRangeAsync(List<T> entities);
    Task<Result<bool>> DeleteAsync(long id);
    Task<Result<bool>> DeleteAsync(T entity);
    Task<Result<List<T>>> GetAllAsync();
    Task<Result<T>> GetByIdAsync(long id);
    Task<Result<T>> UpdateAsync(T entity);
}
