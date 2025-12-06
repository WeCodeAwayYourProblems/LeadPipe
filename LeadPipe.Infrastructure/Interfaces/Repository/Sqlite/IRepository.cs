using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;

namespace LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

public interface IRepository<T> where T : class, IEntity
{
    Task<Result<T>> AddAsync(T entity);
    Task<Result<List<T>>> AddRangeAsync(List<T> entities);
    Task<Result<bool>> DeleteAsync(long id);
    Task<Result<bool>> DeleteAsync(T entity);
    Task<Result<List<T>>> GetAllAsync();
    Task<Result<T>> GetByIdAsync(long id);
    Task<Result<T>> UpdateAsync(T entity);
}