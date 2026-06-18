using CSharpFunctionalExtensions;
using LeadPipe.Core;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository;
using LeadPipe.Infrastructure.Settings;

namespace LeadPipe.Infrastructure.Data.Transform;

public sealed class TransformYellerReport_new(
    IRepositoryFactory factory,
    IYellerSettings settings
) : ITransform<Plumbing, ReportYeller>
{
    private readonly IRepository<PlumbingEntity> _plumbs = factory.GetRepository<PlumbingEntity>();
    private readonly IRepository<CornEntity> _corn = factory.GetRepository<CornEntity>();
    private readonly IRepository<CaliperEntity> _caliper = factory.GetRepository<CaliperEntity>();
    private readonly IRepository<CustardEntity> _custard = factory.GetRepository<CustardEntity>();
    private readonly IYellerSettings _settings = settings;


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "The expression is passed to ef, which can't deal with StringComparison method overloads")]
    public async Task<Result<List<ReportYeller>>> TransformAsync(List<Plumbing> _)
    {
        //*************************************************************************
        // Retrieve all data
        //*************************************************************************
        // Retrieve plumbing
        var plumbs = await _plumbs.FindAsync(p =>
            p.Source == Source.Yeller);
        if (plumbs.IsFailure) return Result.Failure<List<ReportYeller>>(plumbs.Error);

        // Retrieve calipers
        var calipers = await _caliper.FindAsync(c =>
            c.Source == _settings.YellerCaliperSource1 ||
                c.Source == _settings.YellerCaliperSource2);
        if (calipers.IsFailure) return Result.Failure<List<ReportYeller>>(calipers.Error);

        // Retrieve corns
        var corns = await _corn.FindAsync(c =>
            c.Source.ToLower().Contains(_settings.YellerCornSource!.ToLower()) ||
                c.Source == _settings.YellerCaliperSource1 ||
                c.Source == _settings.YellerCaliperSource2);
        if (corns.IsFailure) return Result.Failure<List<ReportYeller>>(corns.Error);

        // Retrieve custards using id lookup
        var plumbingIdDict = plumbs.Value.ToDictionaryFast(x => x.Id);
        var plumbingIds = plumbingIdDict.Keys.ToList();
        var caliperIdDict = calipers.Value.ToDictionaryFast(x => x.Id);
        var caliperIds = caliperIdDict.Keys.ToList();
        var cornIdDict = corns.Value.ToDictionaryFast(x => x.Id);
        var cornIds = cornIdDict.Keys.ToList();

        Result<List<CustardEntity>> custards = await _custard
            .FindWithDetailsAsync(c =>
                c.CustardPlumbingLinks.Any(link => plumbingIds.Contains(link.PlumbingId)) ||
                c.CustardCaliperLinks.Any(link => caliperIds.Contains(link.CaliperId)) ||
                c.CustardCornLinks.Any(link => cornIds.Contains(link.CornId))
            );
        if (custards.IsFailure) return Result.Failure<List<ReportYeller>>(custards.Error);

        //*************************************************************************
        // Find first touch entity
        //*************************************************************************
        // Hydrate links with entities. Only hydrate the first touch per link type
        List<FirstTouchEntityCustard> firsttouches = custards.Value
            .Select(c =>
            {
                CustardPlumbingLink? cPlumb = FirstTouch(c.CustardPlumbingLinks);
                CustardCaliperLink? cCal = FirstTouch(c.CustardCaliperLinks);
                CustardCornLink? cCorn = FirstTouch(c.CustardCornLinks);
                FirstTouchEntityCustard firstTouch = FirstTouch(cPlumb, cCal, cCorn, c);
                return firstTouch;
            })
            .Where(f => f.Entity is not null)
            .ToList();

        //*************************************************************************
        // Attribute Sands appropriately
        //*************************************************************************


        //*************************************************************************
        // Convert custards into intermediary item 
        //*************************************************************************


        //*************************************************************************
        // Translate intermediary item into report
        //*************************************************************************

        return Result.Failure<List<ReportYeller>>("Not Implemented");

    }
    
    record FirstTouchEntityCustard(IPhoneDateIdEntity? Entity, CustardEntity Custard);

    record PrelimYellerReport(IPhoneDateIdEntity Entity, CustardEntity Custard, SandEntity[] Sands);

    private static CustardPlumbingLink? FirstTouch(ICollection<CustardPlumbingLink> c)
    {
        if (c.Count == 0)
            return null;
        var first = c.First();
        foreach (var v in c)
        {
            if (v.UnixMatchDate < first.UnixMatchDate)
                first = v;
        }
        return first;
    }

    private static CustardCaliperLink? FirstTouch(ICollection<CustardCaliperLink> c)
    {
        if (c.Count == 0)
            return null;
        var first = c.First();
        foreach (var v in c)
        {
            if (v.UnixMatchDate < first.UnixMatchDate)
                first = v;
        }
        return first;
    }

    private static CustardCornLink? FirstTouch(ICollection<CustardCornLink> c)
    {
        if (c.Count == 0)
            return null;
        var first = c.First();
        foreach (var v in c)
        {
            if (v.UnixMatchDate < first.UnixMatchDate)
                first = v;
        }
        return first;
    }

    private static FirstTouchEntityCustard FirstTouch(CustardPlumbingLink? cPlumb, CustardCaliperLink? cCal, CustardCornLink? cCorn, CustardEntity c)
    {
        #region local static
        static IPhoneDateIdEntity? find(CustardPlumbingLink cPlumb, CustardCaliperLink cCal, CustardCornLink cCorn)
        {
            // Clear winners
            bool cPlumbFirst = cPlumb.UnixMatchDate < cCal.UnixMatchDate && cPlumb.UnixMatchDate < cCorn.UnixMatchDate;
            if (cPlumbFirst) return cPlumb.Plumbing;

            bool cCalFirst = cCal.UnixMatchDate < cPlumb.UnixMatchDate && cCal.UnixMatchDate < cCorn.UnixMatchDate;
            if (cCalFirst) return cCal.Caliper;

            bool cCornFirst = cCorn.UnixMatchDate < cPlumb.UnixMatchDate && cCorn.UnixMatchDate < cCal.UnixMatchDate;
            if (cCornFirst) return cCorn.Corn;

            // ***********************************
            // Business logic tie breakers

            // plumbcal tie, Favor cal
            bool tiePlumbCal = cPlumb.UnixMatchDate == cCal.UnixMatchDate && cPlumb.UnixMatchDate < cCorn.UnixMatchDate;
            if (tiePlumbCal) return cCal.Caliper;

            // cal corn tie, Favor Cal
            bool tieCalCorn = cCal.UnixMatchDate == cCorn.UnixMatchDate && cCal.UnixMatchDate < cPlumb.UnixMatchDate;
            if (tieCalCorn) return cCal.Caliper;

            // plumbcorn tie, Favor Plumb
            bool tiePlumbCorn = cPlumb.UnixMatchDate == cCorn.UnixMatchDate && cPlumb.UnixMatchDate < cCal.UnixMatchDate;
            if (tiePlumbCorn) return cPlumb.Plumbing;

            // All tied, favor cal
            if (cPlumb.UnixMatchDate == cCal.UnixMatchDate && cPlumb.UnixMatchDate == cCorn.UnixMatchDate)
                return cCal.Caliper;

            return null;
        }
        #endregion

        return (cPlumb, cCal, cCorn) switch
        {
            (null, null, null) => new(null, c),

            (not null, null, null) => new(cPlumb.Plumbing, c),
            (null, not null, null) => new(cCal.Caliper, c),
            (null, null, not null) => new(cCorn.Corn, c),

            (not null, not null, null) => new(cPlumb.UnixMatchDate <= cCal.UnixMatchDate ? cPlumb.Plumbing : cCal.Caliper, c),
            (not null, null, not null) => new(cPlumb.UnixMatchDate <= cCorn.UnixMatchDate ? cPlumb.Plumbing : cCorn.Corn, c),
            (null, not null, not null) => new(cCal.UnixMatchDate <= cCorn.UnixMatchDate ? cCal.Caliper : cCorn.Corn, c),

            (not null, not null, not null) => new(find(cPlumb, cCal, cCorn), c),
        };
    }


}