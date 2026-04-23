using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Core;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository;

namespace LeadPipe.Infrastructure.Service;

internal sealed class EntityAssociationService(IRepositoryFactory repoFactory) : IEntityAssociationService
{
    #region Repo Fields
    private readonly IRepository<CustardEntity> _custardRepo = repoFactory.GetRepository<CustardEntity>();
    private readonly IRepository<SandEntity> _sandRepo = repoFactory.GetRepository<SandEntity>();
    private readonly IRepository<PlumbingEntity> _plumbingRepo = repoFactory.GetRepository<PlumbingEntity>();
    private readonly IRepository<CornEntity> _cornRepo = repoFactory.GetRepository<CornEntity>();
    private readonly IRepository<CaliperEntity> _caliperRepo = repoFactory.GetRepository<CaliperEntity>();
    private readonly IRepository<CustardCaliperLink> _custardCaliperRepo = repoFactory.GetRepository<CustardCaliperLink>();
    private readonly IRepository<CustardCornLink> _custardCornRepo = repoFactory.GetRepository<CustardCornLink>();
    private readonly IRepository<CustardPlumbingLink> _custardPlumbingRepo = repoFactory.GetRepository<CustardPlumbingLink>();
    private readonly IRepository<SandCaliperLink> _sandCaliperRepo = repoFactory.GetRepository<SandCaliperLink>();
    private readonly IRepository<SandCornLink> _sandCornRepo = repoFactory.GetRepository<SandCornLink>();
    private readonly IRepository<SandPlumbingLink> _sandPlumbingRepo = repoFactory.GetRepository<SandPlumbingLink>();
    private readonly IRepository<CornCaliperLink> _cornCaliperRepo = repoFactory.GetRepository<CornCaliperLink>();
    private readonly IRepository<CornPlumbingLink> _cornPlumbingRepo = repoFactory.GetRepository<CornPlumbingLink>();
    private readonly IRepository<PlumbingCaliperLink> _plumbingCaliperRepo = repoFactory.GetRepository<PlumbingCaliperLink>();
    #endregion

    public async Task<Result> AssociateAsync()
    {
        // Load all the entities
        Result<List<CustardEntity>> custards = await _custardRepo.GetAllWithDetailsAsync();
        Result<List<SandEntity>> sands = await _sandRepo.GetAllWithDetailsAsync();
        Result<List<PlumbingEntity>> plumbings = await _plumbingRepo.GetAllWithDetailsAsync();

        Result<List<CornEntity>> corns = await _cornRepo.GetAllWithDetailsAsync();
        Result<List<CaliperEntity>> calipers = await _caliperRepo.GetAllWithDetailsAsync();

        Result entitiesCombined = Result.Combine(custards, sands, plumbings, corns, calipers);
        if (entitiesCombined.IsFailure) return entitiesCombined;

        // Load all the existing links
        Result<List<CornCaliperLink>> cornCaliperLinksResult = await _cornCaliperRepo.GetAllAsync();
        Result<List<PlumbingCaliperLink>> plumbingCaliperLinksResult = await _plumbingCaliperRepo.GetAllAsync();
        Result<List<SandCaliperLink>> sandCaliperLinksResult = await _sandCaliperRepo.GetAllAsync();
        Result<List<CornPlumbingLink>> cornPlumbingLinksResult = await _cornPlumbingRepo.GetAllAsync();
        Result<List<SandPlumbingLink>> sandPlumbingLinksResult = await _sandPlumbingRepo.GetAllAsync();
        Result<List<CustardCaliperLink>> custardCaliperLinksResult = await _custardCaliperRepo.GetAllAsync();
        Result<List<CustardCornLink>> custardCornLinksResult = await _custardCornRepo.GetAllAsync();
        Result<List<CustardPlumbingLink>> custardPlumbingLinksResult = await _custardPlumbingRepo.GetAllAsync();

        Result linksCombined = Result.Combine(
            cornCaliperLinksResult,
            plumbingCaliperLinksResult,
            sandCaliperLinksResult,
            cornPlumbingLinksResult,
            sandPlumbingLinksResult,
            custardCaliperLinksResult,
            custardCornLinksResult,
            custardPlumbingLinksResult
            );
        if (linksCombined.IsFailure) return linksCombined;

        // Convert existing links to hashsets
        HashSet<(long, long)> existingCornCaliper = cornCaliperLinksResult.Value.ToHashSetFast(l => (l.CornId, l.CaliperId));
        HashSet<(long, long)> existingPlumbingCaliper = plumbingCaliperLinksResult.Value.ToHashSetFast(l => (l.PlumbingId, l.CaliperId));
        HashSet<(long, long)> existingSandCaliper = sandCaliperLinksResult.Value.ToHashSetFast(l => (l.SandId, l.CaliperId));
        HashSet<(long, long)> existingCornPlumbing = cornPlumbingLinksResult.Value.ToHashSetFast(l => (l.CornId, l.PlumbingId));
        HashSet<(long, long)> existingSandPlumbing = sandPlumbingLinksResult.Value.ToHashSetFast(l => (l.SandId, l.PlumbingId));
        HashSet<(long, long)> existingCustardCaliper = custardCaliperLinksResult.Value.ToHashSetFast(l => (l.CustardId, l.CaliperId));
        HashSet<(long, long)> existingCustardCorn = custardCornLinksResult.Value.ToHashSetFast(l => (l.CustardId, l.CornId));
        HashSet<(long, long)> existingCustardPlumbing = custardPlumbingLinksResult.Value.ToHashSetFast(l => (l.CustardId, l.PlumbingId));

        // Generate new links
        List<CornCaliperLink> cornCaliperLinks = GenerateCornCaliperLinks(corns.Value, calipers.Value, existingCornCaliper);
        List<PlumbingCaliperLink> plumbingCaliperLinks = GeneratePlumbingCaliperLinks(plumbings.Value, calipers.Value, existingPlumbingCaliper);
        List<SandCaliperLink> sandCaliperLinks = GenerateSandCaliperLinks(sands.Value, calipers.Value, existingSandCaliper);
        List<CornPlumbingLink> cornPlumbingLinks = GenerateCornPlumbingLinks(corns.Value, plumbings.Value, existingCornPlumbing);
        List<SandPlumbingLink> sandPlumbingLinks = GenerateSandPlumbingLinks(sands.Value, plumbings.Value, existingSandPlumbing);
        List<CustardCaliperLink> custardCaliperLinks = GenerateCustardCaliperLinks(custards.Value, calipers.Value, existingCustardCaliper);
        List<CustardCornLink> custardCornLinks = GenerateCustardCornLinks(custards.Value, corns.Value, existingCustardCorn);
        List<CustardPlumbingLink> custardPlumbingLinks = GenerateCustardPlumbingLinks(custards.Value, plumbings.Value, existingCustardPlumbing);

        // Persist new links
        Result cornCaliperUpsert = await _cornCaliperRepo.UpsertRangeAsync(cornCaliperLinks);
        Result plumbingCaliperUpsert = await _plumbingCaliperRepo.UpsertRangeAsync(plumbingCaliperLinks);
        Result sandCaliperUpsert = await _sandCaliperRepo.UpsertRangeAsync(sandCaliperLinks);
        Result cornPlumbingUpsert = await _cornPlumbingRepo.UpsertRangeAsync(cornPlumbingLinks);
        Result sandPlumbingUpsert = await _sandPlumbingRepo.UpsertRangeAsync(sandPlumbingLinks);
        Result custardCaliperUpsert = await _custardCaliperRepo.UpsertRangeAsync(custardCaliperLinks);
        Result custardCornUpsert = await _custardCornRepo.UpsertRangeAsync(custardCornLinks);
        Result custardPlumbingUpsert = await _custardPlumbingRepo.UpsertRangeAsync(custardPlumbingLinks);

        Result combinedUpserts = Result.Combine(
            cornCaliperUpsert,
            plumbingCaliperUpsert,
            sandCaliperUpsert,
            cornPlumbingUpsert,
            sandPlumbingUpsert,
            custardCaliperUpsert,
            custardCornUpsert,
            custardPlumbingUpsert
            );

        return combinedUpserts;
    }

    #region Link Generation

    // Custard to Corn
    private static List<CustardCornLink> GenerateCustardCornLinks(
        List<CustardEntity> custards,
        List<CornEntity> corns,
        HashSet<(long CustardId, long CornId)> existing)
    {
        List<CustardCornLink> results = [];

        // Build Corn lookup by phone
        var byPhone = corns
            .Where(c => c.PhoneNumber != null && c.PhoneNumber.CanParticipateInDeduplication)
            .GroupBy(c => c.PhoneNumber)
            .ToDictionaryFast(
                g => g.Key!,
                g => g.OrderBy(c => c.Date).First() // Finds the chronologically first item by phonenumber
            );

        foreach (CustardEntity e in custards)
        {
            var link = CreateLink(
                e,
                [e.PhoneNumber, e.PhoneNumber2],
                byPhone,
                existing,
                (corn, custard, phone) => new CustardCornLink
                {
                    CustardId = e.Id,
                    CornId = corn.Id,
                    MatchingPhone = phone.Number,
                    UnixMatchDate = corn.UnixDate
                }
            );
            results.AddRange(link);
        }

        return results;
    }

    // Custard to Plumbing
    private static List<CustardPlumbingLink> GenerateCustardPlumbingLinks(
        List<CustardEntity> custards,
        List<PlumbingEntity> plumbings,
        HashSet<(long CustardId, long PlumbingId)> existing)
    {
        var results = new List<CustardPlumbingLink>();

        var byPhone = plumbings
            .Where(c => c.PhoneNumber != null && c.PhoneNumber.CanParticipateInDeduplication)
            .GroupBy(c => c.PhoneNumber)
            .ToDictionaryFast(
                g => g.Key!,
                g => g.OrderBy(c => c.Date).First() // Finds the chronologically first item by phonenumber
            );

        foreach (CustardEntity e in custards)
        {
            var link = CreateLink(
                e,
                [e.PhoneNumber, e.PhoneNumber2],
                byPhone,
                existing,
                (custard, plumbing, phone) => new CustardPlumbingLink
                {
                    CustardId = e.Id,
                    PlumbingId = plumbing.Id,
                    MatchingPhone = phone.Number,
                    UnixMatchDate = plumbing.UnixDate
                }
            );
            results.AddRange(link);
        }

        return results;
    }

    // Custard to Caliper
    private static List<CustardCaliperLink> GenerateCustardCaliperLinks(
        List<CustardEntity> custards,
        List<CaliperEntity> calipers,
        HashSet<(long CustardId, long CaliperId)> existing)
    {
        List<CustardCaliperLink> results = [];

        var byPhone = calipers
            .Where(c => c.PhoneNumber != null && c.PhoneNumber.CanParticipateInDeduplication)
            .GroupBy(c => c.PhoneNumber)
            .ToDictionaryFast(
                g => g.Key!,
                g => g.OrderBy(c => c.Date).First() // Finds the chronologically first item by phonenumber
            );

        foreach (CustardEntity e in custards)
        {
            var link = CreateLink(
                e,
                [e.PhoneNumber, e.PhoneNumber2],
                byPhone,
                existing,
                (custard, caliper, phone) => new CustardCaliperLink
                {
                    CustardId = e.Id,
                    CaliperId = caliper.Id,
                    MatchingPhone = phone.Number,
                    UnixMatchDate = caliper.UnixDate
                }
            );
            results.AddRange(link);
        }

        return results;
    }

    // Sand to Plumbing
    private static List<SandPlumbingLink> GenerateSandPlumbingLinks(
        List<SandEntity> sands,
        List<PlumbingEntity> plumbings,
        HashSet<(long SandId, long PlumbingId)> existing)
    {
        List<SandPlumbingLink> results = [];

        var byPhone = plumbings
            .Where(c => c.PhoneNumber != null && c.PhoneNumber.CanParticipateInDeduplication)
            .GroupBy(c => c.PhoneNumber)
            .ToDictionaryFast(
                g => g.Key!,
                g => g.OrderBy(c => c.Date).First() // Finds the chronologically first item by phonenumber
            );

        foreach (SandEntity e in sands)
        {
            if (e.CustardEntity is null)
                throw new Exception($"{nameof(SandEntity)} cannot have null navigation values to {nameof(e.CustardEntity)}");
            var link = CreateLink(
                e,
                [e.CustardEntity.PhoneNumber, e.CustardEntity.PhoneNumber2],
                byPhone,
                existing,
                (sand, plumbing, phone) => new SandPlumbingLink
                {
                    SandId = e.Id,
                    PlumbingId = plumbing.Id,
                    MatchingPhone = phone.Number,
                    UnixMatchDate = plumbing.UnixDate
                }
             );
            results.AddRange(link);
        }

        return results;
    }

    // Sand to Caliper
    private static List<SandCaliperLink> GenerateSandCaliperLinks(
        List<SandEntity> sands,
        List<CaliperEntity> calipers,
        HashSet<(long SandId, long CaliperId)> existing)
    {
        List<SandCaliperLink> results = [];

        var byPhone = calipers
            .Where(c => c.PhoneNumber != null && c.PhoneNumber.CanParticipateInDeduplication)
            .GroupBy(c => c.PhoneNumber)
            .ToDictionaryFast(
                g => g.Key!,
                g => g.OrderBy(c => c.Date).First() // Finds the chronologically first item by phonenumber
            );

        foreach (SandEntity e in sands)
        {
            if (e.CustardEntity is null)
                continue;

            var link =
            CreateLink(
                e,
                [e.CustardEntity.PhoneNumber, e.CustardEntity.PhoneNumber2],
                byPhone,
                existing,
                (sand, caliper, phone) => new SandCaliperLink
                {
                    SandId = e.Id,
                    CaliperId = caliper.Id,
                    MatchingPhone = phone.Number,
                    UnixMatchDate = caliper.UnixDate
                }
            );
            results.AddRange(link);
        }

        return results;
    }

    // Plumbing to Caliper
    private static List<PlumbingCaliperLink> GeneratePlumbingCaliperLinks(
        List<PlumbingEntity> plumbings,
        List<CaliperEntity> calipers,
        HashSet<(long PlumbingId, long CaliperId)> existing)
    {
        var results = new List<PlumbingCaliperLink>();

        var byPhone = calipers
            .Where(c => c.PhoneNumber != null && c.PhoneNumber.CanParticipateInDeduplication)
            .GroupBy(c => c.PhoneNumber)
            .ToDictionaryFast(
                g => g.Key!,
                g => g.OrderBy(c => c.Date).First() // Finds the chronologically first item by phonenumber
            );

        foreach (var e in plumbings)
        {
            results.AddRange(
            CreateLink(
                e,
                [e.PhoneNumber],
                byPhone,
                existing,
                (plumbing, caliper, phone) => new PlumbingCaliperLink
                {
                    PlumbingId = e.Id,
                    CaliperId = caliper.Id,
                    MatchingPhone = phone.Number,
                    UnixMatchDate = byPhone.TryGetValue(phone, out var cal) && cal.UnixDate < plumbing.UnixDate
                        ? cal.UnixDate
                        : plumbing.UnixDate
                }
            ));
        }

        return results;
    }

    // Corn to Plumbing
    private static List<CornPlumbingLink> GenerateCornPlumbingLinks(
        List<CornEntity> corns,
        List<PlumbingEntity> plumbings,
        HashSet<(long CornId, long PlumbingId)> existing)
    {
        List<CornPlumbingLink> results = [];

        var byPhone = plumbings
            .Where(c => c.PhoneNumber != null && c.PhoneNumber.CanParticipateInDeduplication)
            .GroupBy(c => c.PhoneNumber)
            .ToDictionaryFast(
                g => g.Key!,
                g => g.OrderBy(c => c.Date).First() // Finds the chronologically first item by phonenumber
            );

        foreach (CornEntity e in corns)
        {
            results.AddRange(
            CreateLink(
                e,
                [e.PhoneNumber],
                byPhone,
                existing,
                (corn, plumbing, phone) => new CornPlumbingLink
                {
                    CornId = e.Id,
                    PlumbingId = plumbing.Id,
                    MatchingPhone = phone.Number,
                    UnixMatchDate = byPhone.TryGetValue(phone, out var plumb) && plumb.UnixDate < corn.UnixDate
                        ? plumb.UnixDate
                        : corn.UnixDate
                }
            ));
        }

        return results;
    }

    // Corn to Caliper
    private static List<CornCaliperLink> GenerateCornCaliperLinks(
        List<CornEntity> corns,
        List<CaliperEntity> calipers,
        HashSet<(long CornId, long CaliperId)> existing)
    {
        var results = new List<CornCaliperLink>();

        var byPhone = calipers
            .Where(c => c.PhoneNumber != null && c.PhoneNumber.CanParticipateInDeduplication)
            .GroupBy(c => c.PhoneNumber)
            .ToDictionaryFast(
                g => g.Key!,
                g => g.OrderBy(c => c.Date).First() // Finds the chronologically first item by phonenumber
            );

        foreach (CornEntity e in corns)
        {
            results.AddRange(
            CreateLink(
                e,
                [e.PhoneNumber],
                byPhone,
                existing,
                (corn, caliper, phone) => new CornCaliperLink
                {
                    CornId = e.Id,
                    CaliperId = caliper.Id,
                    MatchingPhone = phone.Number,
                    UnixMatchDate = byPhone.TryGetValue(phone, out var cal) && cal.UnixDate < corn.UnixDate
                        ? cal.UnixDate
                        : corn.UnixDate // Choose the first one between the matching caliper vs corn 
                }
            ));
        }

        return results;
    }

#endregion 

    private static List<TLink> CreateLink<TSource, TTarget, TLink>
    (
        TSource source,
        IEnumerable<PhoneNumber?> phones,
        Dictionary<PhoneNumber, TTarget> lookup,
        HashSet<(long, long)> existing,
        Func<TSource, TTarget, PhoneNumber, TLink> createLink
    )
    where TTarget : IEntity
    where TSource : IEntity
    {
        List<TLink> result = [];
        foreach (var phone in phones)
        {
            if (phone is null || !phone.CanParticipateInDeduplication || !lookup.TryGetValue(phone, out var target))
                continue;

            var key = (source.Id, target.Id);
            if (existing.Contains(key))
                continue;

            result.Add(createLink(source, target, phone));
        }
        return result;
    }
}
