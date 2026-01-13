using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

namespace LeadPipe.Infrastructure.Service;

internal class PlumbingAssociationService(
    IPlumbingRepository plumbingRepo,
    ISandRepository sandRepo,
    ICaliperRepository caliperRepo,
    ISandPlumbingLinkRepository linkRepo,
    ISandCaliperLinkRepository sandCaliperRepo,
    IPlumbingCaliperLinkRepository plumbingCaliperRepo
    ) : IPlumbingAssociationService
{
    #region Private
    private readonly IPlumbingRepository _plumbingRepo = plumbingRepo;
    private readonly ISandRepository _sandRepo = sandRepo;
    private readonly ICaliperRepository _caliperRepo = caliperRepo;

    private readonly ISandPlumbingLinkRepository _sandPlumbingRepo = linkRepo;
    private readonly ISandCaliperLinkRepository _sandCaliperRepo = sandCaliperRepo;
    private readonly IPlumbingCaliperLinkRepository _plumbingCaliperRepo = plumbingCaliperRepo;

    #endregion

    public async Task<Result> SaveLinksAsync()
    {
        // Fetch base entities
        var plumbingResult = await _plumbingRepo.GetAllAsync();
        var sandResult = await _sandRepo.GetAllAsync();
        var caliperResult = await _caliperRepo.GetAllAsync();

        var combined = Result.Combine(plumbingResult, sandResult, caliperResult);
        if (combined.IsFailure)
            return combined;

        var plumbingEntities = plumbingResult.Value;
        var sandEntities = sandResult.Value;
        var caliperEntities = caliperResult.Value;

        // Build lookup dictionaries (deduped safely)
        var plumbingByPhone = plumbingEntities
            .GroupBy(p => p.PhoneNumber)
            .ToDictionary(g => g.Key, g => g.Last());

        var caliperByPhone = caliperEntities
            .GroupBy(c => c.PhoneNumber)
            .ToDictionary(g => g.Key, g => g.Last());

        var sandById = sandEntities.ToDictionary(s => s.Id);
        var plumbingById = plumbingEntities.ToDictionary(p => p.Id);
        var caliperById = caliperEntities.ToDictionary(c => c.Id);

        // Load existing links
        var existingSandPlumbing = await LoadExistingSandPlumbingAsync();
        var existingSandCaliper = await LoadExistingSandCaliperAsync();
        var existingPlumbingCaliper = await LoadExistingPlumbingCaliperAsync();

        // Generate new links
        var sandPlumbingLinks = GenerateSandPlumbingLinks(
            sandEntities,
            plumbingByPhone,
            sandById,
            existingSandPlumbing);

        var sandCaliperLinks = GenerateSandCaliperLinks(
            sandEntities,
            caliperByPhone,
            sandById,
            existingSandCaliper);

        var plumbingCaliperLinks = GeneratePlumbingCaliperLinks(
            plumbingEntities,
            caliperByPhone,
            plumbingById,
            existingPlumbingCaliper);

        // Persist
        var saveSandPlumbing = await _sandPlumbingRepo.UpsertRangeAsync(sandPlumbingLinks);
        var saveSandCaliper = await _sandCaliperRepo.UpsertRangeAsync(sandCaliperLinks);
        var savePlumbingCaliper = await _plumbingCaliperRepo.UpsertRangeAsync(plumbingCaliperLinks);

        return Result.Combine(saveSandPlumbing, saveSandCaliper, savePlumbingCaliper);
    }
    private async Task<HashSet<(long SandId, long PlumbingId)>> LoadExistingSandPlumbingAsync()
    {
        var result = await _sandPlumbingRepo.GetAllAsync();
        return result.IsSuccess
            ? [.. result.Value.Select(l => (l.SandId, l.PlumbingId))]
            : [];
    }

    private async Task<HashSet<(long SandId, long CaliperId)>> LoadExistingSandCaliperAsync()
    {
        var result = await _sandCaliperRepo.GetAllAsync();
        return result.IsSuccess
            ? [.. result.Value.Select(l => (l.SandId, l.CaliperId))]
            : [];
    }

    private async Task<HashSet<(long PlumbingId, long CaliperId)>> LoadExistingPlumbingCaliperAsync()
    {
        var result = await _plumbingCaliperRepo.GetAllAsync();
        return result.IsSuccess
            ? [.. result.Value.Select(l => (l.PlumbingId, l.CaliperId))]
            : [];
    }
    private static List<SandPlumbingLink> GenerateSandPlumbingLinks(
        List<SandEntity> Sand,
        Dictionary<long, PlumbingEntity> plumbingByPhone,
        Dictionary<long, SandEntity> SandById,
        HashSet<(long SandId, long PlumbingId)> existing)
    {
        var links = new List<SandPlumbingLink>();

        foreach (var s in Sand)
        {
            foreach (var number in new[] { s.PhoneNumber, s.PhoneNumber2 })
            {
                if (!plumbingByPhone.TryGetValue(number, out var plumbing))
                    continue;

                var key = (s.Id, plumbing.Id);
                if (existing.Contains(key))
                    continue;

                links.Add(new SandPlumbingLink
                {
                    SandId = s.Id,
                    SandEntity = SandById[s.Id],
                    PlumbingId = plumbing.Id,
                    PlumbingEntity = plumbing,
                    MatchingPhone = number
                });
            }
        }

        return links;
    }
    private static List<SandCaliperLink> GenerateSandCaliperLinks(
        List<SandEntity> sand,
        Dictionary<long, CaliperEntity> caliperByPhone,
        Dictionary<long, SandEntity> sandById,
        HashSet<(long SandId, long CaliperId)> existing)
    {
        var links = new List<SandCaliperLink>();

        foreach (var s in sand)
        {
            foreach (var number in new[] { s.PhoneNumber, s.PhoneNumber2 })
            {
                if (!caliperByPhone.TryGetValue(number, out var caliper))
                    continue;

                var key = (s.Id, caliper.Id);
                if (existing.Contains(key))
                    continue;

                links.Add(new SandCaliperLink
                {
                    SandId = s.Id,
                    SandEntity = sandById[s.Id],
                    CaliperId = caliper.Id,
                    CaliperEntity = caliper,
                    MatchingPhone = number
                });
            }
        }

        return links;
    }
    private static List<PlumbingCaliperLink> GeneratePlumbingCaliperLinks(
        List<PlumbingEntity> plumbing,
        Dictionary<long, CaliperEntity> caliperByPhone,
        Dictionary<long, PlumbingEntity> plumbingById,
        HashSet<(long PlumbingId, long CaliperId)> existing)
    {
        var links = new List<PlumbingCaliperLink>();

        foreach (var p in plumbing)
        {
            if (!caliperByPhone.TryGetValue(p.PhoneNumber, out var caliper))
                continue;

            var key = (p.Id, caliper.Id);
            if (existing.Contains(key))
                continue;

            links.Add(new PlumbingCaliperLink
            {
                PlumbingId = p.Id,
                PlumbingEntity = plumbingById[p.Id],
                CaliperId = caliper.Id,
                CaliperEntity = caliper
            });
        }

        return links;
    }

}
