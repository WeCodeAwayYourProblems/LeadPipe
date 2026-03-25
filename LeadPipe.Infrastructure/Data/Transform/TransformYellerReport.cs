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
    IEntityToYellerReportFactory reportFactory,
    IYellerSettings settings
    ) : ITransform<Plumbing, ReportYeller>
{

    private readonly IRepository<CustardEntity> _custardRepo = factory.GetRepository<CustardEntity>();
    private readonly IRepository<CaliperEntity> _caliperRepo = factory.GetRepository<CaliperEntity>();
    private readonly IRepository<CornEntity> _cornRepo = factory.GetRepository<CornEntity>();
    private readonly IRepository<PlumbingEntity> _plumbRepo = factory.GetRepository<PlumbingEntity>();

    private readonly IEntityToReport<AttributionResult, ReportYeller> _attrToR = reportFactory.GetService<AttributionResult>();
    private readonly IEntityToReport<CornEntity, ReportYeller> _cornToR = reportFactory.GetService<CornEntity>();
    private readonly IEntityToReport<PlumbingEntity, ReportYeller> _plumbToR = reportFactory.GetService<PlumbingEntity>();
    private readonly IEntityToReport<CaliperEntity, ReportYeller> _caliperToR = reportFactory.GetService<CaliperEntity>();

    private readonly IYellerSettings _settings = settings;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "The expression is passed to ef, which can't deal with StringComparison method overloads")]
    public async Task<Result<List<ReportYeller>>> TransformAsync(List<Plumbing> data)
    {
        //*********************************************************************************
        // Load All Relevant data
        //*********************************************************************************

        Result<List<CornEntity>> corns =
            await _cornRepo.FindAsync(c =>
                c.Source.ToLower().Contains(_settings.YellerCornSource!.ToLower()) ||
                c.Source == _settings.YellerCaliperSource1 ||
                c.Source == _settings.YellerCaliperSource2);
        if (corns.IsFailure) return Result.Failure<List<ReportYeller>>(corns.Error);


        Result<List<CaliperEntity>> calipers =
            await _caliperRepo.FindAsync(c =>
                c.Source == _settings.YellerCaliperSource1 ||
                c.Source == _settings.YellerCaliperSource2);
        if (calipers.IsFailure) return Result.Failure<List<ReportYeller>>(calipers.Error);

        Result<List<PlumbingEntity>> plumbs =
            await _plumbRepo.FindAsync(c =>
                c.Source == Source.Yeller);
        if (plumbs.IsFailure) return Result.Failure<List<ReportYeller>>(plumbs.Error);

        HashSet<long> cornLookup = [.. corns.Value.Select(x => x.Id)];
        HashSet<long> plumbLookup = [.. data.Select(x => x.Id)];
        HashSet<long> caliperLookup = [.. calipers.Value.Select(x => x.Id)];

        // Load custards with details
        Result<List<CustardEntity>> custardsResult = await _custardRepo.FindWithDetailsAsync(c =>
                c.CustardPlumbingLinks.Any(link => plumbLookup.Contains(link.PlumbingId)) ||
                c.CustardCaliperLinks.Any(link => caliperLookup.Contains(link.CaliperId)) ||
                c.CustardCornLinks.Any(link => cornLookup.Contains(link.CornId))
            );
        if (custardsResult.IsFailure) return Result.Failure<List<ReportYeller>>(custardsResult.Error);

        //*********************************************************************************
        // Flatten sands: keep only first chronological sand per custard
        //*********************************************************************************

        // Any entity matching the custard is non-attributable when ANY sand or custard date is before the entity date
        // Only the first sand is relevant. It also has to be completed, but that is determined later
        var custards = Result.Success(
            custardsResult.Value
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

        // We've loaded all the custards, regardless of how they're associated, and we've loaded all calipers, corns, and plumbs regardless of association
        // Here, we're putting custards and calipers together in a record
        Dictionary<long, CaliperEntity> caliperById = calipers.Value.ToDictionary(c => c.Id);
        Dictionary<long, CornEntity> cornById = corns.Value.ToDictionary(c => c.Id);
        Dictionary<long, PlumbingEntity> plumbById = plumbs.Value.ToDictionary(c => c.Id);

        // For every custard,
        // And every link in every custard,
        // If the entity is from yeller, then associate them together
        // But not all custards have calipers that have a yeller source, so we eliminate those custards because they're not associated
        var custardCaliperAssociations =
            from custard in custards.Value
            from link in custard.CustardCaliperLinks
            let entity = caliperById.TryGetValue(link.CaliperId, out var caliper) ? caliper : null
            where entity != null
            select new CustardAssociation<CaliperEntity>(entity!, custard, entity!.PhoneNumber.Number, entity!.UnixDate);

        var custardCornAssociations =
            from custard in custards.Value
            from link in custard.CustardCornLinks
            let entity = cornById.TryGetValue(link.CornId, out var corn) ? corn : null
            where entity != null
            select new CustardAssociation<CornEntity>(entity!, custard, entity!.PhoneNumber.Number, entity!.UnixDate);

        var custardPlumbAssociations =
            from custard in custards.Value
            from link in custard.CustardPlumbingLinks
            let entity = plumbById.TryGetValue(link.PlumbingId, out var plumb) ? plumb : null
            where entity != null
            select new CustardAssociation<PlumbingEntity>(entity!, custard, entity!.PhoneNumber.Number, entity!.UnixDate);

        // Here, some custards are not attributable because the custard's first sand entity associated is not completed, 
        List<CustardAssociation<CaliperEntity>> caliperAttributable = FilterAttributable(custardCaliperAssociations);
        List<CustardAssociation<CornEntity>> cornAttributable = FilterAttributable(custardCornAssociations);
        List<CustardAssociation<PlumbingEntity>> plumbAttributable = FilterAttributable(custardPlumbAssociations);

        //*********************************************************************************
        // Cross-entity first-touch filter
        //*********************************************************************************

        // Gather all attributable associations together
        List<(long MatchingPhone, long UnixMatchDate, CustardEntity Custard, AttributionSource Source, IPhoneDateIdEntity Entity)> allTouches =
        [
            .. plumbAttributable.Select(a => (a.EntityPhone, a.EntityDate, a.Custard, AttributionSource.Plumbing, a.Entity)),
            .. cornAttributable.Select(a => (a.EntityPhone, a.EntityDate, a.Custard, AttributionSource.Corn, a.Entity)),
            .. caliperAttributable.Select(a => (a.EntityPhone, a.EntityDate, a.Custard, AttributionSource.Caliper, a.Entity))
        ];

        // Cast associations into Effective Date Associated
        // For each custard, earliest effective date = min(link.UnixMatchDate, custard.UnixDate, firstSandDate)
        // Earliest sand regardless of completion affects tie-breaking
        List<EffectiveDateAssociated> touchesWithEffectiveDate = [.. allTouches
            .Select(t =>
            {
                long firstSandDate = t.Custard.SandEntities?.Min(s => s.UnixDate) ?? long.MaxValue;
                long effectiveDate = Math.Min(t.UnixMatchDate, Math.Min(firstSandDate, t.Custard.UnixDate));
                return new EffectiveDateAssociated(t.MatchingPhone, t.UnixMatchDate, t.Custard, EffectiveDate: effectiveDate, t.Source, t.Entity);
            })];

        // Get the first effectiveDateAssociated by phone number. This gives us first touch by phone number
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

        // Convert All custards that have the same EntityDate, Custard.UnixDate, and SandEntities.Single().UnixDate across them
        // These are ready for translation into the report
        List<AttributionResult> attributions = [.. firstTouchesByPhone
            .SelectMany(d => d.Value.Select(v =>
                new AttributionResult()
                {
                    MatchingPhone = v.MatchingPhone,
                    FirstTouchUnixDate = v.UnixMatchDate,
                    Source = v.Source,
                    Entity = v.Entity,
                    Custard = v.Custard,
                    Sand = v.Custard.SandEntities.Single(),
                }
            ))];

        //*********************************************************************************
        // Non attributed reporting
        //*********************************************************************************

        // Find first touch by phone number across all entities for non-attributed reporting
        // Group by phone number and find the first one
        Dictionary<long, CaliperEntity> firstCalipers = calipers.Value
            .GroupBy(c => c.PhoneNumber.Number)
            .ToDictionary(
                g => g.Key,
                g => g
                    .OrderBy(c => c.UnixDate)
                    .ThenBy(c => c.Id)
                    .First()
            );

        Dictionary<long, CornEntity> firstCorns = corns.Value
            .GroupBy(c => c.PhoneNumber.Number)
            .ToDictionary(
                g => g.Key,
                g => g
                    .OrderBy(c => c.UnixDate)
                    .ThenBy(c => c.Id)
                    .First()
            );

        Dictionary<long, PlumbingEntity> firstPlumbing = plumbs.Value
            .GroupBy(c => c.PhoneNumber.Number)
            .ToDictionary(
                g => g.Key,
                g => g
                    .OrderBy(c => c.UnixDate)
                    .ThenBy(c => c.Id)
                    .First()
            );

        // Put all first touch dictionaries together.
        // Find the first by phone number, regardless of entity
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
        // This allows us to create a partition between attributed vs non attributed
        var attributionByPhone = attributions
            .GroupBy(a => a.MatchingPhone)
            .ToDictionary(g => g.Key, g => g.ToList());

        List<ReportYeller> reports = [.. crossEntityFirstTouches.Keys
            .SelectMany(phone =>
                attributionByPhone.TryGetValue(phone, out var winners)
                    ? winners.Select(_attrToR.Translate)
                    :
                    [
                        crossEntityFirstTouches[phone] switch
                        {
                            CaliperEntity c => _caliperToR.Translate(c),
                            CornEntity c => _cornToR.Translate(c),
                            PlumbingEntity p => _plumbToR.Translate(p),
                            _ => throw new InvalidOperationException("Unknown entity type")
                        }
                    ]
            )];

        return reports;

    }

    // If we have multiple custard-entity associations with the same custard id, we find the first touch of that entity by its entity date
    // Then we ensure that only associations where the association is attributable, defined as
    // 1. Sands cannot be null or empty.
    // 2. The first sand must be complete.
    // 3. The entity must be before BOTH the custard and sand date
    // And we're removing any associations that are not attributable
    private static List<CustardAssociation<TEntity>> FilterAttributable<TEntity>(IEnumerable<CustardAssociation<TEntity>> associations)
    {
        var result = associations
            .GroupBy(a => a.Custard.Id)
            .Select(g =>
            {
                CustardAssociation<TEntity>? earliest = g.MinBy(a => a.EntityDate);
                return earliest != null && IsAttributable(earliest)
                    ? earliest
                    : null;
            })
            .OfType<CustardAssociation<TEntity>>()
            .ToList();
        return result;
    }

    private static bool IsAttributable<T>(CustardAssociation<T> a)
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
               a.EntityDate < firstSandDate!;
    }

    private record CustardAssociation<T>(T Entity, CustardEntity Custard, long EntityPhone, long EntityDate);
    private record EffectiveDateAssociated(long MatchingPhone, long UnixMatchDate, CustardEntity Custard, long EffectiveDate, AttributionSource Source, IPhoneDateIdEntity Entity);
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