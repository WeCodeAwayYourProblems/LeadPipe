using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;

namespace LeadPipe.Infrastructure.Interfaces.Repository;

/// <summary>
/// Saves the timestamp and success state of the last run for a particular <see cref="SyncKey"/>
/// </summary>
public interface ISyncStampRepository
{
    Task<Result<SyncStampEntity>> GetByKeyAsync(Source? source, SyncKey key);
    Task<Result<SyncStampEntity>> UpsertAsync(SyncStampEntity entity);
}