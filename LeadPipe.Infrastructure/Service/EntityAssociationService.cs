using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

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
        HashSet<(long, long)> existingCornCaliper = [.. cornCaliperLinksResult.Value.Select(l => (l.CornId, l.CaliperId))];
        HashSet<(long, long)> existingPlumbingCaliper = [.. plumbingCaliperLinksResult.Value.Select(l => (l.PlumbingId, l.CaliperId))];
        HashSet<(long, long)> existingSandCaliper = [.. sandCaliperLinksResult.Value.Select(l => (l.SandId, l.CaliperId))];
        HashSet<(long, long)> existingCornPlumbing = [.. cornPlumbingLinksResult.Value.Select(l => (l.CornId, l.PlumbingId))];
        HashSet<(long, long)> existingSandPlumbing = [.. sandPlumbingLinksResult.Value.Select(l => (l.SandId, l.PlumbingId))];
        HashSet<(long, long)> existingCustardCaliper = [.. custardCaliperLinksResult.Value.Select(l => (l.CustardId, l.CaliperId))];
        HashSet<(long, long)> existingCustardCorn = [.. custardCornLinksResult.Value.Select(l => (l.CustardId, l.CornId))];
        HashSet<(long, long)> existingCustardPlumbing = [.. custardPlumbingLinksResult.Value.Select(l => (l.CustardId, l.PlumbingId))];

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

    // Custard to Corn
    private static List<CustardCornLink> GenerateCustardCornLinks(List<CustardEntity> custards, List<CornEntity> corns, HashSet<(long CustardId, long CornId)> existing)
    {
        List<CustardCornLink> results = [];

        // Build Corn lookup by phone
        Dictionary<long, CornEntity> cornByPhone = corns
            .GroupBy(c => c.PhoneNumber)
            .ToDictionary(g => g.Key, g => g.Last());

        foreach (CustardEntity c in custards)
        {
            foreach (long phone in new[] { c.PhoneNumber, c.PhoneNumber2 })
            {
                if (!cornByPhone.TryGetValue(phone, out var corn))
                    continue;

                var key = (c.Id, corn.Id);
                if (existing.Contains(key))
                    continue;

                results.Add(new CustardCornLink
                {
                    CustardId = c.Id,
                    CornId = corn.Id,
                    MatchingPhone = phone
                });
            }
        }

        return results;
    }

    // Custard to Plumbing
    private static List<CustardPlumbingLink> GenerateCustardPlumbingLinks(List<CustardEntity> custards, List<PlumbingEntity> plumbings, HashSet<(long CustardId, long PlumbingId)> existing)
    {
        var results = new List<CustardPlumbingLink>();

        var plumbingByPhone = plumbings
            .GroupBy(p => p.PhoneNumber)
            .ToDictionary(g => g.Key, g => g.Last());

        foreach (CustardEntity c in custards)
        {
            foreach (long phone in new[] { c.PhoneNumber, c.PhoneNumber2 })
            {
                if (!plumbingByPhone.TryGetValue(phone, out var plumbing))
                    continue;

                var key = (c.Id, plumbing.Id);
                if (existing.Contains(key))
                    continue;

                results.Add(new CustardPlumbingLink
                {
                    CustardId = c.Id,
                    PlumbingId = plumbing.Id,
                    MatchingPhone = phone
                });
            }
        }

        return results;
    }

    // Custard to Caliper
    private static List<CustardCaliperLink> GenerateCustardCaliperLinks(List<CustardEntity> custards, List<CaliperEntity> calipers, HashSet<(long CustardId, long CaliperId)> existing)
    {
        List<CustardCaliperLink> results = [];

        Dictionary<long, CaliperEntity> caliperByPhone = calipers
            .GroupBy(c => c.PhoneNumber)
            .ToDictionary(g => g.Key, g => g.Last());

        foreach (var c in custards)
        {
            foreach (var phone in new[] { c.PhoneNumber, c.PhoneNumber2 })
            {
                if (!caliperByPhone.TryGetValue(phone, out var caliper))
                    continue;

                var key = (c.Id, caliper.Id);
                if (existing.Contains(key))
                    continue;

                results.Add(new CustardCaliperLink
                {
                    CustardId = c.Id,
                    CaliperId = caliper.Id,
                    MatchingPhone = phone
                });
            }
        }

        return results;
    }

    // Sand to Plumbing
    private static List<SandPlumbingLink> GenerateSandPlumbingLinks(List<SandEntity> sands, List<PlumbingEntity> plumbings, HashSet<(long SandId, long PlumbingId)> existing)
    {
        List<SandPlumbingLink> results = [];

        var plumbingByPhone = plumbings
            .GroupBy(p => p.PhoneNumber)
            .ToDictionary(g => g.Key, g => g.Last());

        foreach (SandEntity s in sands)
        {
            if (s.CustardEntity is null)
                throw new Exception($"{nameof(SandEntity)} cannot have null navigation values to {nameof(s.CustardEntity)}");

            foreach (long phone in new[] { s.CustardEntity.PhoneNumber, s.CustardEntity.PhoneNumber2 })
            {
                if (!plumbingByPhone.TryGetValue(phone, out var plumbing))
                    continue;

                var key = (s.Id, plumbing.Id);
                if (existing.Contains(key))
                    continue;

                results.Add(new SandPlumbingLink
                {
                    SandId = s.Id,
                    PlumbingId = plumbing.Id,
                    MatchingPhone = phone
                });
            }
        }

        return results;
    }

    // Sand to Caliper
    private static List<SandCaliperLink> GenerateSandCaliperLinks(List<SandEntity> sands, List<CaliperEntity> calipers, HashSet<(long SandId, long CaliperId)> existing)
    {
        List<SandCaliperLink> results = [];

        Dictionary<long, CaliperEntity> caliperByPhone = calipers
            .GroupBy(c => c.PhoneNumber)
            .ToDictionary(g => g.Key, g => g.Last());

        foreach (SandEntity s in sands)
        {
            if (s.CustardEntity is null)
                throw new Exception($"{nameof(SandEntity)} cannot have null navigation values to {nameof(s.CustardEntity)}");

            foreach (long phone in new[] { s.CustardEntity.PhoneNumber, s.CustardEntity.PhoneNumber2 })
            {
                if (!caliperByPhone.TryGetValue(phone, out var caliper))
                    continue;

                var key = (s.Id, caliper.Id);
                if (existing.Contains(key))
                    continue;

                results.Add(new SandCaliperLink
                {
                    SandId = s.Id,
                    CaliperId = caliper.Id,
                    MatchingPhone = phone
                });
            }
        }

        return results;
    }

    // Plumbing to Caliper
    private static List<PlumbingCaliperLink> GeneratePlumbingCaliperLinks(List<PlumbingEntity> plumbings, List<CaliperEntity> calipers, HashSet<(long PlumbingId, long CaliperId)> existing)
    {
        var results = new List<PlumbingCaliperLink>();

        var caliperByPhone = calipers.GroupBy(c => c.PhoneNumber)
                                     .ToDictionary(g => g.Key, g => g.Last());

        foreach (var p in plumbings)
        {
            if (!caliperByPhone.TryGetValue(p.PhoneNumber, out var caliper))
                continue;

            var key = (p.Id, caliper.Id);
            if (existing.Contains(key))
                continue;

            results.Add(new PlumbingCaliperLink
            {
                PlumbingId = p.Id,
                CaliperId = caliper.Id,
                MatchingPhone = p.PhoneNumber
            });
        }

        return results;
    }

    // Corn to Plumbing
    private static List<CornPlumbingLink> GenerateCornPlumbingLinks(List<CornEntity> corns, List<PlumbingEntity> plumbings, HashSet<(long CornId, long PlumbingId)> existing)
    {
        var results = new List<CornPlumbingLink>();

        var plumbingByPhone = plumbings.GroupBy(p => p.PhoneNumber)
                                       .ToDictionary(g => g.Key, g => g.Last());

        foreach (var c in corns)
        {
            if (!plumbingByPhone.TryGetValue(c.PhoneNumber, out var plumbing))
                continue;

            var key = (c.Id, plumbing.Id);
            if (existing.Contains(key))
                continue;

            results.Add(new CornPlumbingLink
            {
                CornId = c.Id,
                PlumbingId = plumbing.Id,
                MatchingPhone = c.PhoneNumber
            });
        }

        return results;
    }

    // Corn to Caliper
    private static List<CornCaliperLink> GenerateCornCaliperLinks(List<CornEntity> corns, List<CaliperEntity> calipers, HashSet<(long CornId, long CaliperId)> existing)
    {
        var results = new List<CornCaliperLink>();

        var caliperByPhone = calipers.GroupBy(c => c.PhoneNumber)
                                     .ToDictionary(g => g.Key, g => g.Last());

        foreach (var c in corns)
        {
            if (!caliperByPhone.TryGetValue(c.PhoneNumber, out var caliper))
                continue;

            var key = (c.Id, caliper.Id);
            if (existing.Contains(key))
                continue;

            results.Add(new CornCaliperLink
            {
                CornId = c.Id,
                CaliperId = caliper.Id,
                MatchingPhone = c.PhoneNumber
            });
        }

        return results;
    }
}
