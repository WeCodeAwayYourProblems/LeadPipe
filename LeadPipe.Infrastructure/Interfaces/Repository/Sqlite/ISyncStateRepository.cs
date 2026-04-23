using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;

namespace LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

/// <summary>
/// Saves the date of the actual piece of data, which is identified by a <see cref="BusinessId"/>
/// </summary>
public interface ISyncStateRepository
{
    Task<Result<List<SyncStateEntity>>> UpsertRangeAsync(List<SyncStateEntity> entities);
    Task<Result<SyncStateEntity>> GetByKeyAsync(Source? source, SyncKey key);
    Task<Result<SyncStateEntity>> GetByIdAsync(BusinessId id);
}
