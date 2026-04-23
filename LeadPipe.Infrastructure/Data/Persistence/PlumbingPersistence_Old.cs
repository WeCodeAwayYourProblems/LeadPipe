using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal class PlumbingPersistence_Old(
    IRepository<PlumbingEntity> plumbing,
    IRepository<PlumbingPhoneNumber> phone
    ) : IDataPersistence<PlumbingEntity>
{
    private readonly IRepository<PlumbingEntity> _plumbing = plumbing;
    private readonly IRepository<PlumbingPhoneNumber> _phone = phone;
    public async Task<Result> SaveAsync(List<PlumbingEntity> t)
    {
        List<PlumbingPhoneNumber> phonesToUpsert = t
            .Where(p => p.PhoneNumbers is not null && p.PhoneNumbers.Count > 0)
            .SelectMany(p => p.PhoneNumbers)
            .ToList();

        Result<List<PlumbingEntity>> upsertedPlumbingEntities = await _plumbing.UpsertRangeAsync(t);

        // If there are no phones to upsert, no need to proceed.
        if (upsertedPlumbingEntities.IsFailure || phonesToUpsert.Count == 0)
            return upsertedPlumbingEntities;

        Result<List<PlumbingPhoneNumber>> upsertedPhoneNumbers = await _phone.UpsertRangeAsync(phonesToUpsert);
        if (upsertedPhoneNumbers.IsFailure)
            return Result.Failure($"{nameof(PlumbingEntity)} list was upserted, but {nameof(PlumbingPhoneNumber)} failed to upsert: {upsertedPhoneNumbers.Error}");

        return Result.Success();
    }
}
