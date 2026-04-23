using CSharpFunctionalExtensions;
using LeadPipe.Core;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal class PlumbingPersistence(
    IRepository<PlumbingEntity> plumbing,
    IPlumbingMetaDataCanonicalPersistenceFormat<PlumbingEntity, string> metaTranslate,
    IRepository<PlumbingPhoneNumber> phone
    ) : IDataPersistence<PlumbingEntity>
{
    private readonly IRepository<PlumbingEntity> _plumbing = plumbing;
    private readonly IRepository<PlumbingPhoneNumber> _phone = phone;
    private readonly IPlumbingMetaDataCanonicalPersistenceFormat<PlumbingEntity, string> _metaTranslate = metaTranslate;

    public async Task<Result> SaveAsync(List<PlumbingEntity> t)
    {
        var phonesToUpsert = t
            .Where(p => p.PhoneNumbers is not null && p.PhoneNumbers.Count > 0)
            .ToDictionaryFast(
                p => new PlumbingKey(p.PhoneNumber, p.UnixDate, p.Source, _metaTranslate.Translate(p)),
                p => p.PhoneNumbers);

        Dictionary<PlumbingKey, PlumbingEntity> inputMap = t
            .GroupBy(p => new PlumbingKey(p.PhoneNumber, p.UnixDate, p.Source, _metaTranslate.Translate(p)))
            .ToDictionaryFast(g => g.Key, g => g.First());
        
        List<PlumbingEntity> uniqueEntities = [.. inputMap.Values];

        Result<List<PlumbingEntity>> result = await _plumbing.UpsertRangeAsync(uniqueEntities);

        // If there are no phones to upsert, no need to proceed.
        if (result.IsFailure || phonesToUpsert.Count == 0)
            return result;

        List<PhoneNumber> inputNumbers = [.. t.Select(t => t.PhoneNumber).Distinct()];
        List<Source> inputSources = [.. t.Select(t => t.Source).Distinct()];
        long minDate = t.Min(t => t.UnixDate);

        Result<List<PlumbingEntity>> retrieved = await _plumbing.FindAsync(p =>
            inputNumbers.Contains(p.PhoneNumber) &&
            inputSources.Contains(p.Source) &&
            p.UnixDate >= minDate
        );
        if (retrieved.IsFailure)
            return Result.Failure($"Failed to retrieve {nameof(PlumbingEntity)} list for {nameof(PlumbingPhoneNumber)} attribution sequence: {retrieved.Error}");

        Dictionary<PlumbingKey, PlumbingEntity> dbMap = retrieved.Value.ToDictionaryFast(r =>
            new PlumbingKey(r.PhoneNumber, r.UnixDate, r.Source, _metaTranslate.Translate(r))
        );

        List<PlumbingPhoneNumber> phones = [];
        foreach (var (key, inputEntity) in inputMap)
        {
            if (!dbMap.TryGetValue(key, out var persisted))
                continue;

            if (!phonesToUpsert.TryGetValue(key, out var numbers))
                continue;

            foreach (var phone in numbers)
            {
                phone.PlumbingId = persisted.Id;
                phones.Add(phone);
            }
        }

        Result<List<PlumbingPhoneNumber>> phoneResult = await _phone.UpsertRangeAsync(phones);
        if (phoneResult.IsFailure)
            return Result.Failure($"Upserted {nameof(PlumbingEntity)} list, retrieved it for the generated ids, but failed to upsert {nameof(PlumbingPhoneNumber)} list: {phoneResult.Error}");

        return Result.Success();
    }
    /// <summary>
    /// This has to match the unique constraint of the <see cref="PlumbingEntity"/> configuration file
    /// </summary>
    /// <param name="PhoneNumber"></param>
    /// <param name="UnixDate"></param>
    /// <param name="Source"></param>
    /// <param name="MetaData"></param>
    record PlumbingKey(PhoneNumber PhoneNumber, long UnixDate, Source Source, string MetaData);
}