using LeadPipe.Infrastructure.Interfaces.Core;

namespace LeadPipe.Infrastructure.Interfaces.Repository;

public interface IRepositoryFactory
{
    IRepository<TEntity> GetRepository<TEntity>() where TEntity : class, IEntity;
}