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
    IYellerSettings settings
    ) : ITransform<Plumbing, ReportYeller>
{

    private readonly IRepository<CustardEntity> _custardRepo = factory.GetRepository<CustardEntity>();
    private readonly IRepository<CaliperEntity> _caliperRepo = factory.GetRepository<CaliperEntity>();
    private readonly IRepository<CornEntity> _cornRepo = factory.GetRepository<CornEntity>();
    private readonly IRepository<PlumbingEntity> _plumbRepo = factory.GetRepository<PlumbingEntity>();

    private readonly IEntityToReport<AttributionResult, ReportYeller> _translate = translate;

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

        HashSet<long> plumbLookup = data.Select(x => x.Id).ToHashSet();
        HashSet<long> caliperLookup = calipers.Value.Select(x => x.Id).ToHashSet();
        HashSet<long> cornLookup = corns.Value.Select(x => x.Id).ToHashSet();

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
                    // .OrderBy().First() = O(n log n)
                    // This is effectively O(n) + O(n)
                    long minDate = c.SandEntities
                        .Min(s => s.UnixDate);
                    SandEntity firstSand = c.SandEntities
                        .First(s => s.UnixDate == minDate);
                    c.SandEntities = [firstSand]; // keep only the earliest sand 
                    return c;
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
            let caliper = caliperById[link.CaliperId]
            select new CustardAssociation<CaliperEntity>(caliper, custard, caliper.PhoneNumber.Number, caliper.UnixDate);

        var custardCornAssociations =
            from custard in custards.Value
            from link in custard.CustardCornLinks
            let corn = cornById[link.CornId]
            select new CustardAssociation<CornEntity>(corn, custard, corn.PhoneNumber.Number, corn.UnixDate);

        var custardPlumbAssociations =
            from custard in custards.Value
            from link in custard.CustardPlumbingLinks
            let plumb = plumbById[link.PlumbingId]
            select new CustardAssociation<PlumbingEntity>(plumb, custard, plumb.PhoneNumber.Number, plumb.UnixDate);

        List<CustardAssociation<CaliperEntity>> caliperAttributable = Attributable(custardCaliperAssociations);
        List<CustardAssociation<CornEntity>> cornAttributable = Attributable(custardCornAssociations);
        List<CustardAssociation<PlumbingEntity>> plumbAttributable = Attributable(custardPlumbAssociations);

        //*********************************************************************************
        // Cross-entity first-touch filter
        //*********************************************************************************

        List<(long MatchingPhone, long UnixMatchDate, CustardEntity Custard, AttributionSource Source)> allTouches =
        [
            .. plumbAttributable.Select(a => (a.EntityPhone, a.EntityDate, a.Custard, AttributionSource.Plumbing)),
            .. cornAttributable.Select(a => (a.EntityPhone, a.EntityDate, a.Custard, AttributionSource.Corn)),
            .. caliperAttributable.Select(a => (a.EntityPhone, a.EntityDate, a.Custard, AttributionSource.Caliper))
        ];

        // For each custard, earliest effective date = min(link.UnixMatchDate, custard.UnixDate, firstSandDate)
        List<EffectiveDateAssociated> touchesWithEffectiveDate = allTouches.Select(t =>
        {
            long firstSandDate = t.Custard.SandEntities?.Min(s => s.UnixDate) ?? long.MaxValue;
            long effectiveDate = Math.Min(t.UnixMatchDate, Math.Min(firstSandDate, t.Custard.UnixDate));
            return new EffectiveDateAssociated(t.MatchingPhone, t.UnixMatchDate, t.Custard, EffectiveDate: effectiveDate, t.Source);
        }).ToList();

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


        List<AttributionResult> attributions = firstTouchesByPhone
            .SelectMany(d => d.Value.Select(v => // We are translating All custards that have the same EntityDate, Custard.UnixDate, and SandEntities.Single().UnixDate across them
                new AttributionResult()
                {
                    MatchingPhone = v.MatchingPhone,
                    FirstTouchUnixDate = v.UnixMatchDate,
                    Source = v.Source,
                    Custard = v.Custard,
                    Sand = v.Custard.SandEntities.Single()
                }
            )).ToList();

        var reports = attributions.Select(_translate.Translate).ToList();

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
    record EffectiveDateAssociated(long MatchingPhone, long UnixMatchDate, CustardEntity Custard, long EffectiveDate, AttributionSource Source);
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