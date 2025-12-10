using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;

namespace LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

public interface ISyncStateRepository
{
    Task<Result<SyncStateEntity>> GetAsync();
    Task<Result<SyncStateEntity>> SaveAsync(SyncStateEntity entity);
}

