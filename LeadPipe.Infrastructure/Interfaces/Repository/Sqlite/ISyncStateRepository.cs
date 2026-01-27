using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;

namespace LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

public interface ISyncStateRepository
{
    Task<Result<List<SyncStateEntity>>> UpsertRangeAsync(List<SyncStateEntity> entities);
    Task<Result<SyncStateEntity>> GetByIdAsync(BusinessId id);
}

