using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Infrastructure.Settings;

namespace LeadPipe.Infrastructure.Data.Transform;

internal sealed class TransformYellerReport(
    IRepositoryFactory factory,
    IEntityToReport<CustardPlumbingLink, ReportYeller> cpLinkToR,
    IEntityToReport<CustardCaliperLink, ReportYeller> custCalToR,
    IEntityToReport<CustardCornLink, ReportYeller> custCornToR,
    IYellerSettings settings
    ) : ITransform<Plumbing, ReportYeller>
{

    private readonly IRepository<CustardEntity> _custardRepo = factory.GetRepository<CustardEntity>();
    private readonly IRepository<CaliperEntity> _caliperRepo = factory.GetRepository<CaliperEntity>();
    private readonly IRepository<CornEntity> _cornRepo = factory.GetRepository<CornEntity>();
    private readonly IRepository<PlumbingEntity> _plumbRepo = factory.GetRepository<PlumbingEntity>();

    private readonly IEntityToReport<CustardPlumbingLink, ReportYeller> _cpLinkToR = cpLinkToR;
    private readonly IEntityToReport<CustardCaliperLink, ReportYeller> _custCalToR = custCalToR;
    private readonly IEntityToReport<CustardCornLink, ReportYeller> _custCornToR = custCornToR;

    private readonly IYellerSettings _settings = settings;

    public async Task<Result<List<ReportYeller>>> TransformAsync(List<Plumbing> data)
    {
        //*********************************************************************************
        // Load All Relevant data
        //*********************************************************************************

        // Load all entities relevant to Yeller
        // These entities do not need navigation properties loaded
        Result<List<CaliperEntity>> calipers = 
            await _caliperRepo.FindAsync(c => 
                c.Source == _settings.YellerCaliperSource1 || 
                c.Source == _settings.YellerCaliperSource2); // This source is a specific string
        if (calipers.IsFailure) return Result.Failure<List<ReportYeller>>(calipers.Error);

        Result<List<CornEntity>> corns = 
            await _cornRepo.FindAsync(c => 
                c.Source == _settings.YellerCornSource); // This source is a specific string
        if (corns.IsFailure) return Result.Failure<List<ReportYeller>>(corns.Error);

        Result<List<PlumbingEntity>> plumbs = 
            await _plumbRepo.FindAsync(c => 
                c.Source == Domain.ValueObjects.Source.Yeller); // This source is an enum with db conversion to string
        if (plumbs.IsFailure) return Result.Failure<List<ReportYeller>>(plumbs.Error);

        HashSet<long> plumbLookup = [.. data.Select(x => x.Id)];
        HashSet<long> caliperLookup = [.. calipers.Value.Select(x => x.Id)];
        HashSet<long> cornLookup = [.. corns.Value.Select(x => x.Id)];

        // Load CustardEntities --> getting them with details is less expensive than getting the details separately
        Result<List<CustardEntity>> custards = await _custardRepo.FindWithDetailsAsync(c =>
                c.CustardPlumbingLinks.Any(link => plumbLookup.Contains(link.PlumbingId)) ||
                c.CustardCaliperLinks.Any(link => caliperLookup.Contains(link.CaliperId)) ||
                c.CustardCornLinks.Any(link => cornLookup.Contains(link.CornId))
            ); // Retrieves RELEVANT custard entities instead of ALL custard entities
        if (custards.IsFailure) return Result.Failure<List<ReportYeller>>(custards.Error);

        //*********************************************************************************
        // Associations
        //*********************************************************************************

        // All entities associated with the same custard must be qualified based on date
        // The first entity attributable to a specific custard wins the value
        // To be attributable, the entity date must
        // 1. Be earlier than all custard dates, including sand dates, 
        // 2. Be earlier than all other entities

        // Build dictionaries for quick lookup
        Dictionary<long, CaliperEntity> caliperById = 
            calipers.Value.ToDictionary(c => c.Id);
        Dictionary<long, CornEntity> cornById = 
            corns.Value.ToDictionary(c => c.Id);
        Dictionary<long, PlumbingEntity> plumbById = 
            plumbs.Value.ToDictionary(c => c.Id);

        // Flatten the relationship between a custard, <link>, and <entity>
        IEnumerable<CustardAssociation<CaliperEntity, CustardCaliperLink>> custardCaliperAssociations =
            from custard in custards.Value
            from link in custard.CustardCaliperLinks
            let caliper = caliperById[link.CaliperId]
            select new CustardAssociation<CaliperEntity, CustardCaliperLink>(link, caliper, custard, caliper.Date);

        IEnumerable<CustardAssociation<CornEntity, CustardCornLink>> custardCornAssociations =
            from custard in custards.Value
            from link in custard.CustardCornLinks
            let corn = cornById[link.CornId]
            select new CustardAssociation<CornEntity, CustardCornLink>(link, corn, custard, corn.Date);

        IEnumerable<CustardAssociation<PlumbingEntity, CustardPlumbingLink>> custardPlumbAssociations =
            from custard in custards.Value
            from link in custard.CustardPlumbingLinks
            let plumb = plumbById[link.PlumbingId]
            select new CustardAssociation<PlumbingEntity, CustardPlumbingLink>(link, plumb, custard, plumb.Date);

        // Attributable associations
        IEnumerable<CustardAssociation<CaliperEntity, CustardCaliperLink>> caliperAttributable = Attributable(custardCaliperAssociations);
        IEnumerable<CustardAssociation<CornEntity, CustardCornLink>> cornAttributable = Attributable(custardCornAssociations);
        IEnumerable<CustardAssociation<PlumbingEntity, CustardPlumbingLink>> plumbAttributable = Attributable(custardPlumbAssociations);

        //*********************************************************************************
        // Report
        //*********************************************************************************

        // Report attributable associations
        List<ReportYeller> reports =
        [
            .. caliperAttributable.Select(c => _custCalToR.Translate(c.Link)),
            .. cornAttributable.Select(c => _custCornToR.Translate(c.Link)),
            .. plumbAttributable.Select(c => _cpLinkToR.Translate(c.Link))
        ];

        return reports;

    }

    static IEnumerable<CustardAssociation<TEntity, TLink>> Attributable<TEntity, TLink>(IEnumerable<CustardAssociation<TEntity, TLink>> associations)
    {
        IEnumerable<CustardAssociation<TEntity, TLink>> result = associations
            .GroupBy(a => a.Custard.Id)
            .Select(g =>
            {
                var earliest = g.MinBy(a => a.EntityDate);
                return earliest is not null && IsAttributable(earliest)
                    ? earliest
                    : null;
            })
            .OfType<CustardAssociation<TEntity, TLink>>();
        return result;
    }

    static bool IsAttributable<T, TLink>(CustardAssociation<T, TLink> a)
    {
        IEnumerable<DateTime> cutoffDates =
            a.Custard.SandEntities.Select(s => s.Date)
            .Append(a.Custard.Date);
        bool result = a.EntityDate < cutoffDates.Min();
        return result;
    }

    record CustardAssociation<T, TLink>(TLink Link, T Entity, CustardEntity Custard, DateTime EntityDate);

}
