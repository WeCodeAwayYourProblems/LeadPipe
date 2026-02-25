using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Infrastructure.Settings;

namespace LeadPipe.Infrastructure.Data.Transform;

internal sealed class TransformYellerReport(
    IRepositoryFactory factory,
    IEntityToReport<AttributionResult, ReportYeller> translate,
    IEntityToReport<CornEntity, ReportYeller> cornToR,
    IEntityToReport<PlumbingEntity, ReportYeller> plumbToR,
    IEntityToReport<CaliperEntity, ReportYeller> caliperToR,
    IYellerSettings settings
    ) : ITransform<Plumbing, ReportYeller>
{

    private readonly IRepository<CustardEntity> _custardRepo = factory.GetRepository<CustardEntity>();
    private readonly IRepository<CaliperEntity> _caliperRepo = factory.GetRepository<CaliperEntity>();
    private readonly IRepository<CornEntity> _cornRepo = factory.GetRepository<CornEntity>();
    private readonly IRepository<PlumbingEntity> _plumbRepo = factory.GetRepository<PlumbingEntity>();

    private readonly IEntityToReport<AttributionResult, ReportYeller> _attrToR = translate;
    private readonly IEntityToReport<CornEntity, ReportYeller> _cornToR = cornToR;
    private readonly IEntityToReport<PlumbingEntity, ReportYeller> _plumbToR = plumbToR;
    private readonly IEntityToReport<CaliperEntity, ReportYeller> _caliperToR = caliperToR;

    private readonly IYellerSettings _settings = settings;

    public async Task<Result<List<ReportYeller>>> TransformAsync(List<Plumbing> data)
    {
        //*********************************************************************************
        // Load All Relevant data
        //*********************************************************************************

        Result<List<CaliperEntity>> calipers =
            await _caliperRepo.FindAsync(c =>
                c.Source == _settings.YellerCaliperSource1 ||
                c.Source == _settings.YellerCaliperSource2);
        if (calipers.IsFailure) return Result.Failure<List<ReportYeller>>(calipers.Error);

        Result<List<CornEntity>> corns =
            await _cornRepo.FindAsync(c =>
                c.Source == _settings.YellerCornSource);
        if (corns.IsFailure) return Result.Failure<List<ReportYeller>>(corns.Error);

        Result<List<PlumbingEntity>> plumbs =
            await _plumbRepo.FindAsync(c =>
                c.Source == Domain.ValueObjects.Source.Yeller);
        if (plumbs.IsFailure) return Result.Failure<List<ReportYeller>>(plumbs.Error);

        HashSet<long> plumbLookup = [.. data.Select(x => x.Id)];
        HashSet<long> caliperLookup = [.. calipers.Value.Select(x => x.Id)];
        HashSet<long> cornLookup = [.. corns.Value.Select(x => x.Id)];

        // Load custards with details
        Result<List<CustardEntity>> custards = await _custardRepo.FindWithDetailsAsync(c =>
                c.CustardPlumbingLinks.Any(link => plumbLookup.Contains(link.PlumbingId)) ||
                c.CustardCaliperLinks.Any(link => caliperLookup.Contains(link.CaliperId)) ||
                c.CustardCornLinks.Any(link => cornLookup.Contains(link.CornId))
            );
        if (custards.IsFailure) return Result.Failure<List<ReportYeller>>(custards.Error);

        //*********************************************************************************
        // Flatten sands: keep only first chronological sand per custard
        //*********************************************************************************

        // Any entity matching the custard is non-attributable when ANY sand or custard date is before the entity date
        custards = Result.Success(
            custards.Value
                .Where(c => c.SandEntities != null && c.SandEntities.Count != 0) // filter out null/empty sands
                .Select(c =>
                {
                    CustardEntity result = c.Clone();

                    // .OrderBy().First() = O(n log n)
                    // This is effectively O(n) + O(n)
                    long minDate = c.SandEntities
                        .Min(s => s.UnixDate);
                    SandEntity firstSand = c.SandEntities
                        .First(s => s.UnixDate == minDate);
                    result.SandEntities = [firstSand]; // keep only the earliest sand 
                    return result;
                }).ToList()
        );

        //*********************************************************************************
        // Associations
        //*********************************************************************************

        Dictionary<long, CaliperEntity> caliperById = calipers.Value.ToDictionary(c => c.Id);
        Dictionary<long, CornEntity> cornById = corns.Value.ToDictionary(c => c.Id);
        Dictionary<long, PlumbingEntity> plumbById = plumbs.Value.ToDictionary(c => c.Id);

        var custardCaliperAssociations =
            from custard in custards.Value
            from link in custard.CustardCaliperLinks
            let entity = caliperById.TryGetValue(link.CaliperId, out var caliper) ? caliper : null
            where entity != null
            select new CustardAssociation<CaliperEntity>(entity!, custard, entity.PhoneNumber.Number, entity.UnixDate);

        var custardCornAssociations =
            from custard in custards.Value
            from link in custard.CustardCornLinks
            let entity = cornById.TryGetValue(link.CornId, out var corn) ? corn : null
            where entity != null
            select new CustardAssociation<CornEntity>(entity!, custard, entity.PhoneNumber.Number, entity.UnixDate);

        var custardPlumbAssociations =
            from custard in custards.Value
            from link in custard.CustardPlumbingLinks
            let entity = plumbById.TryGetValue(link.PlumbingId, out var plumb) ? plumb : null
            where entity != null
            select new CustardAssociation<PlumbingEntity>(entity!, custard, entity.PhoneNumber.Number, entity.UnixDate);

        List<CustardAssociation<CaliperEntity>> caliperAttributable = Attributable(custardCaliperAssociations);
        List<CustardAssociation<CornEntity>> cornAttributable = Attributable(custardCornAssociations);
        List<CustardAssociation<PlumbingEntity>> plumbAttributable = Attributable(custardPlumbAssociations);

        //*********************************************************************************
        // Cross-entity first-touch filter
        //*********************************************************************************

        List<(long MatchingPhone, long UnixMatchDate, CustardEntity Custard, AttributionSource Source, IEntity Entity)> allTouches =
        [
            .. plumbAttributable.Select(a => (a.EntityPhone, a.EntityDate, a.Custard, AttributionSource.Plumbing, a.Entity)),
            .. cornAttributable.Select(a => (a.EntityPhone, a.EntityDate, a.Custard, AttributionSource.Corn, a.Entity)),
            .. caliperAttributable.Select(a => (a.EntityPhone, a.EntityDate, a.Custard, AttributionSource.Caliper, a.Entity))
        ];

        // For each custard, earliest effective date = min(link.UnixMatchDate, custard.UnixDate, firstSandDate)
        // Earliest sand regardless of completion affects tie-breaking
        List<EffectiveDateAssociated> touchesWithEffectiveDate = [.. allTouches
            .Select(t =>
            {
                long firstSandDate = t.Custard.SandEntities?.Min(s => s.UnixDate) ?? long.MaxValue;
                long effectiveDate = Math.Min(t.UnixMatchDate, Math.Min(firstSandDate, t.Custard.UnixDate));
                return new EffectiveDateAssociated(t.MatchingPhone, t.UnixMatchDate, t.Custard, EffectiveDate: effectiveDate, t.Source, t.Entity);
            })];

        Dictionary<long, List<EffectiveDateAssociated>> firstTouchesByPhone = touchesWithEffectiveDate
            .GroupBy(t => t.MatchingPhone)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    // Step 1: find the minimum effective date
                    long earliestEffective = g.Min(t => t.EffectiveDate);

                    // Step 2: filter only entities with that earliest effective date
                    IEnumerable<EffectiveDateAssociated> earliestEntities = g.Where(t => t.EffectiveDate == earliestEffective);

                    // Null propagations protect us from null sandentities. Redundant but safe
                    // Step 3: compute "custard min date" for tie-breaking
                    long earliestCustardMinDate = earliestEntities
                        .Min(t => Math.Min(t.Custard.UnixDate, t.Custard.SandEntities?.Single().UnixDate ?? long.MaxValue));

                    // Step 4: only take entities where custard min date matches the earliest
                    List<EffectiveDateAssociated> result = [.. earliestEntities
                        .Where(t => Math.Min(t.Custard.UnixDate, t.Custard.SandEntities?.Single().UnixDate ?? long.MaxValue) == earliestCustardMinDate)
                    ];

                    return result;
                });


        List<AttributionResult> attributions = [.. firstTouchesByPhone
            .SelectMany(d => d.Value.Select(v => // We are translating All custards that have the same EntityDate, Custard.UnixDate, and SandEntities.Single().UnixDate across them
                new AttributionResult()
                {
                    MatchingPhone = v.MatchingPhone,
                    FirstTouchUnixDate = v.UnixMatchDate,
                    Source = v.Source,
                    Custard = v.Custard,
                    Sand = v.Custard.SandEntities.Single(),
                }
            ))];

        //*********************************************************************************
        // Non attributed reporting
        //*********************************************************************************
        
        // Find first touch by phone number across all entities for non-attributed reporting
        var firstCalipers = calipers.Value
            .GroupBy(c => c.PhoneNumber.Number)
            .ToDictionary(
                g => g.Key,
                g => g
                    .OrderBy(c => c.UnixDate)
                    .ThenBy(c => c.Id)
                    .First()
            );

        var firstCorns = corns.Value
            .GroupBy(c => c.PhoneNumber.Number)
            .ToDictionary(
                g => g.Key,
                g => g
                    .OrderBy(c => c.UnixDate)
                    .ThenBy(c => c.Id)
                    .First()
            );
        var firstPlumbing = plumbs.Value
            .GroupBy(c => c.PhoneNumber.Number)
            .ToDictionary(
                g => g.Key,
                g => g
                    .OrderBy(c => c.UnixDate)
                    .ThenBy(c => c.Id)
                    .First()
            );

        Dictionary<long, IPhoneDateIdEntity> crossEntityFirstTouches = firstCalipers.Values
            .Cast<IPhoneDateIdEntity>()
            .Concat(firstCorns.Values)
            .Concat(firstPlumbing.Values)
            .GroupBy(e => e.PhoneNumber.Number)
            .ToDictionary(
                g => g.Key,
                g => g
                    .OrderBy(e => e.UnixDate)
                    .ThenBy(e => e.Id)
                    .First()
            );

        // Build lookup of attribution winners by phone
        var attributionByPhone = attributions
            .GroupBy(a => a.MatchingPhone)
            .ToDictionary(g => g.Key, g => g.ToList());

        List<ReportYeller> reports = [];

        // We want one report per phone
        foreach (var phone in crossEntityFirstTouches.Keys)
        {
            if (attributionByPhone.TryGetValue(phone, out var winners))
            {
                // Attribution overrides raw first-touch
                reports.AddRange(winners.Select(_attrToR.Translate));
            }
            else
            {
                // Where there's no attribution, use cross-entity first touch
                var entity = crossEntityFirstTouches[phone];

                reports.Add(entity switch
                {
                    CaliperEntity c => _caliperToR.Translate(c),
                    CornEntity c => _cornToR.Translate(c),
                    PlumbingEntity p => _plumbToR.Translate(p),
                    _ => throw new InvalidOperationException("Unknown entity type")
                });
            }
        }

        return reports;

    }

    // Respect first sand after entity and Completed == true
    static List<CustardAssociation<TEntity>> Attributable<TEntity>(IEnumerable<CustardAssociation<TEntity>> associations)
    {
        var result = associations
            .GroupBy(a => a.Custard.Id)
            .Select(g =>
            {
                var earliest = g.MinBy(a => a.EntityDate);
                return earliest != null && IsAttributable(earliest)
                    ? earliest
                    : null;
            })
            .OfType<CustardAssociation<TEntity>>()
            .ToList();
        return result;
    }

    static bool IsAttributable<T>(CustardAssociation<T> a)
    {
        var sands = a.Custard.SandEntities;
        if (sands == null || sands.Count == 0) // This is redundant, for safety
            return false;

        long? firstSandDate = sands
            .Where(s => s.Complete) // We MUST filter by complete here, because that's an attribution rule
            .Select(s => (long?)s.UnixDate) // Casting prevents Min from throwing
            .Min();

        if (firstSandDate is null)
            return false;

        return a.EntityDate < a.Custard.UnixDate &&
               a.EntityDate < firstSandDate.Value;
    }


    record CustardAssociation<T>(T Entity, CustardEntity Custard, long EntityPhone, long EntityDate);
    record EffectiveDateAssociated(long MatchingPhone, long UnixMatchDate, CustardEntity Custard, long EffectiveDate, AttributionSource Source, IEntity Entity);
}

#region Logic map
/* No need to ask ai to help you understand what's going on!
Simply look at this diagram
                ┌─────────────────────────────┐
                │         ENTITIES            │
                │  Plumbing | Corn | Caliper  │
                └──────────────┬──────────────┘
                               │ (via link tables)
                               ▼
                ┌─────────────────────────────┐
                │          CUSTARDS           │
                │  1 custard can link to      │
                │  multiple entities          │
                └──────────────┬──────────────┘
                               │ (1 → many)
                               ▼
                ┌─────────────────────────────┐
                │            SANDS            │
                │  - Must be Complete         │
                │  - Only earliest counts     │
                └──────────────┬──────────────┘
                               │
                               ▼
                ┌─────────────────────────────┐
                │   PER-CUSTARD ATTRIBUTION   │
                │  - Earliest entity only     │
                │  - Entity < Custard         │
                │  - Entity < First Sand      │
                └──────────────┬──────────────┘
                               │
                               ▼
                ┌─────────────────────────────┐
                │ CROSS-ENTITY FIRST TOUCH    │
                │  Per Phone Number:          │
                │  - Compute Effective Date   │
                │  - Earliest wins            │
                │  - Tie → allow multiples    │
                └──────────────┬──────────────┘
                               │
                               ▼
                ┌─────────────────────────────┐
                │           REPORT            │
                │  - One winner per phone     │
                │  - Sand value attached      │
                │  - No double counting       │
                └─────────────────────────────┘

*/
#endregion