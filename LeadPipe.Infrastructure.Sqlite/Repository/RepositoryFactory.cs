using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class RepositoryFactory(IServiceProvider provider) : IRepositoryFactory
{
    private readonly IServiceProvider _provider = provider;
    public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class, IEntity
    {
        return _provider.GetRequiredService<IRepository<TEntity>>();
    }
}