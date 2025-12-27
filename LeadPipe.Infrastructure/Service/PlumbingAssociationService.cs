using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Service;

internal class PlumbingAssociationService(
    IPlumbingRepository plumbingRepo,
    ISubsRepository subsRepo,
    ICallRepository callRepo,
    ISubsPlumbingLinkRepository linkRepo,
    ISubsCallLinkRepository subsCallRepo,
    IPlumbingCallLinkRepository plumbingCallRepo
    ) : IPlumbingAssociationService
{
    #region Private
    private readonly IPlumbingRepository _plumbingRepo = plumbingRepo;
    private readonly ISubsRepository _subsRepo = subsRepo;
    private readonly ICallRepository _callRepo = callRepo;

    private readonly ISubsPlumbingLinkRepository _subsPlumbingRepo = linkRepo;
    private readonly ISubsCallLinkRepository _subsCallRepo = subsCallRepo;
    private readonly IPlumbingCallLinkRepository _plumbingCallRepo = plumbingCallRepo;

    #endregion

    public async Task<Result> SaveLinksAsync()
    {
        // Fetch base entities
        var plumbingResult = await _plumbingRepo.GetAllAsync();
        var subsResult = await _subsRepo.GetAllAsync();
        var callResult = await _callRepo.GetAllAsync();

        var combined = Result.Combine(plumbingResult, subsResult, callResult);
        if (combined.IsFailure)
            return combined;

        var plumbingEntities = plumbingResult.Value;
        var subsEntities = subsResult.Value;
        var callEntities = callResult.Value;

        // Build lookup dictionaries (deduped safely)
        var plumbingByPhone = plumbingEntities
            .GroupBy(p => p.PhoneNumber)
            .ToDictionary(g => g.Key, g => g.Last());

        var callByPhone = callEntities
            .GroupBy(c => c.PhoneNumber)
            .ToDictionary(g => g.Key, g => g.Last());

        var subsById = subsEntities.ToDictionary(s => s.Id);
        var plumbingById = plumbingEntities.ToDictionary(p => p.Id);
        var callById = callEntities.ToDictionary(c => c.Id);

        // Load existing links
        var existingSubsPlumbing = await LoadExistingSubsPlumbingAsync();
        var existingSubsCall = await LoadExistingSubsCallAsync();
        var existingPlumbingCall = await LoadExistingPlumbingCallAsync();

        // Generate new links
        var subsPlumbingLinks = GenerateSubsPlumbingLinks(
            subsEntities,
            plumbingByPhone,
            subsById,
            existingSubsPlumbing);

        var subsCallLinks = GenerateSubsCallLinks(
            subsEntities,
            callByPhone,
            subsById,
            existingSubsCall);

        var plumbingCallLinks = GeneratePlumbingCallLinks(
            plumbingEntities,
            callByPhone,
            plumbingById,
            existingPlumbingCall);

        // Persist
        var saveSubsPlumbing = await _subsPlumbingRepo.UpsertRangeAsync(subsPlumbingLinks);
        var saveSubsCall = await _subsCallRepo.UpsertRangeAsync(subsCallLinks);
        var savePlumbingCall = await _plumbingCallRepo.UpsertRangeAsync(plumbingCallLinks);

        return Result.Combine(saveSubsPlumbing, saveSubsCall, savePlumbingCall);
    }
    private async Task<HashSet<(long SubsId, long PlumbingId)>> LoadExistingSubsPlumbingAsync()
    {
        var result = await _subsPlumbingRepo.GetAllAsync();
        return result.IsSuccess
            ? result.Value.Select(l => (l.SubsId, l.PlumbingId)).ToHashSet()
            : [];
    }

    private async Task<HashSet<(long SubsId, long CallId)>> LoadExistingSubsCallAsync()
    {
        var result = await _subsCallRepo.GetAllAsync();
        return result.IsSuccess
            ? result.Value.Select(l => (l.SubsId, l.CallId)).ToHashSet()
            : [];
    }

    private async Task<HashSet<(long PlumbingId, long CallId)>> LoadExistingPlumbingCallAsync()
    {
        var result = await _plumbingCallRepo.GetAllAsync();
        return result.IsSuccess
            ? result.Value.Select(l => (l.PlumbingId, l.CallId)).ToHashSet()
            : [];
    }
    private static List<SubsPlumbingLink> GenerateSubsPlumbingLinks(
        List<SubsEntity> subs,
        Dictionary<long, PlumbingEntity> plumbingByPhone,
        Dictionary<long, SubsEntity> subsById,
        HashSet<(long SubsId, long PlumbingId)> existing)
    {
        var links = new List<SubsPlumbingLink>();

        foreach (var s in subs)
        {
            foreach (var number in new[] { s.Number, s.Number2 })
            {
                if (!plumbingByPhone.TryGetValue(number, out var plumbing))
                    continue;

                var key = (s.Id, plumbing.Id);
                if (existing.Contains(key))
                    continue;

                links.Add(new SubsPlumbingLink
                {
                    SubsId = s.Id,
                    SubsEntity = subsById[s.Id],
                    PlumbingId = plumbing.Id,
                    PlumbingEntity = plumbing,
                    MatchingSubPhone = number
                });
            }
        }

        return links;
    }
    private static List<CallSubsLink> GenerateSubsCallLinks(
        List<SubsEntity> subs,
        Dictionary<long, CallEntity> callByPhone,
        Dictionary<long, SubsEntity> subsById,
        HashSet<(long SubsId, long CallId)> existing)
    {
        var links = new List<CallSubsLink>();

        foreach (var s in subs)
        {
            foreach (var number in new[] { s.Number, s.Number2 })
            {
                if (!callByPhone.TryGetValue(number, out var call))
                    continue;

                var key = (s.Id, call.Id);
                if (existing.Contains(key))
                    continue;

                links.Add(new CallSubsLink
                {
                    SubsId = s.Id,
                    SubsEntity = subsById[s.Id],
                    CallId = call.Id,
                    CallEntity = call,
                    MatchingNumber = number
                });
            }
        }

        return links;
    }
    private static List<PlumbingCallLink> GeneratePlumbingCallLinks(
        List<PlumbingEntity> plumbing,
        Dictionary<long, CallEntity> callByPhone,
        Dictionary<long, PlumbingEntity> plumbingById,
        HashSet<(long PlumbingId, long CallId)> existing)
    {
        var links = new List<PlumbingCallLink>();

        foreach (var p in plumbing)
        {
            if (!callByPhone.TryGetValue(p.PhoneNumber, out var call))
                continue;

            var key = (p.Id, call.Id);
            if (existing.Contains(key))
                continue;

            links.Add(new PlumbingCallLink
            {
                PlumbingId = p.Id,
                PlumbingEntity = plumbingById[p.Id],
                CallId = call.Id,
                CallEntity = call
            });
        }

        return links;
    }

}
