using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

namespace LeadPipe.Infrastructure.Data.Persistence;

public abstract class Persistence<TRepo, TEntity>(TRepo repo) : IDataPersistence<TEntity>
    where TRepo : class, IRepository<TEntity>
    where TEntity : class, IEntity
{
    private readonly TRepo _repo = repo;
    public async Task<Result> SaveAsync(List<TEntity> t)
    {
        Result<List<TEntity>> added = await _repo.UpsertRangeAsync(t); 
        return added;
    }
}
