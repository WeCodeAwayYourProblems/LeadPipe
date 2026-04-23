using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Interfaces.Core;
using System.Linq.Expressions;

namespace LeadPipe.Infrastructure.Interfaces.Repository;

public interface IRepository<TEntity>
    where TEntity : class, IEntity
{
    Task<Result<List<TEntity>>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);

    Task<Result<List<TEntity>>> FindWithDetailsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);

    Task<Result<List<TEntity>>> GetAllAsync(CancellationToken ct = default);

    Task<Result<List<TEntity>>> GetAllWithDetailsAsync(CancellationToken ct = default);

    Task<Result<List<TEntity>>> UpsertRangeAsync(List<TEntity> entities, CancellationToken ct = default);
}
