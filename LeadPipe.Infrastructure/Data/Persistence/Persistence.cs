using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces;
using LeadPipe.Infrastructure.Interfaces.Repository;

namespace LeadPipe.Infrastructure.Data.Persistence;

public abstract class Persistence<IRepo, TEntity>(IRepo repo) : IDataPersistence<TEntity>
    where IRepo : class, IRepository<TEntity>
    where TEntity : class, IEntity
{
    private readonly IRepo _repo = repo;
    public async Task<Result> SaveAsync(List<TEntity> t)
    {
        Result<List<TEntity>> added = await _repo.AddRangeAsync(t); return added;
    }
}
