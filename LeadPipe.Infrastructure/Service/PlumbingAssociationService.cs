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
        Result<List<PlumbingEntity>> plumbingEntityResult = await _plumbingRepo.GetAllAsync();
        Result<List<SubsEntity>> subsEntityResult = await _subsRepo.GetAllAsync();
        Result<List<CallEntity>> callEntityResult = await _callRepo.GetAllAsync();
        
        Result combined = Result.Combine(plumbingEntityResult, subsEntityResult, callEntityResult);
        if (combined.IsFailure)
            return combined;

        // Convert VOs to Entities
        List<PlumbingEntity> plumbingEntities = plumbingEntityResult.Value;
        List<SubsEntity> subsEntities = subsEntityResult.Value;
        List<CallEntity> callEntities = callEntityResult.Value;

        // Create dictionaries for quick lookup by PhoneNumber
        Dictionary<long, PlumbingEntity> plumbingDict = plumbingEntities.ToDictionary(p => p.PhoneNumber);
        Dictionary<long, CallEntity> callDict = callEntities.ToDictionary(c => c.PhoneNumber);

        // Fetch existing links to avoid duplication
        Result<List<SubsPlumbingLink>> existingSubsPlumbingResult = await _subsPlumbingRepo.GetAllAsync();
        HashSet<(long SubsId, long PlumbingId)> existingSubsPlumbing = existingSubsPlumbingResult.IsSuccess
            ? [.. existingSubsPlumbingResult.Value.Select(l => (l.SubsId, l.PlumbingId))]
            : [];

        Result<List<CallSubsLink>> existingSubsCallResult = await _subsCallRepo.GetAllAsync();
        HashSet<(long SubsId, long CallId)> existingSubsCall = existingSubsCallResult.IsSuccess
            ? [.. existingSubsCallResult.Value.Select(l => (l.SubsId, l.CallId))]
            : [];

        Result<List<PlumbingCallLink>> existingPlumbingCallResult = await _plumbingCallRepo.GetAllAsync();
        HashSet<(long PlumbingId, long CallId)> existingPlumbingCall = existingPlumbingCallResult.IsSuccess
            ? [.. existingPlumbingCallResult.Value.Select(l => (l.PlumbingId, l.CallId))]
            : [];

        // Generate Subs-Plumbing links
        List<SubsPlumbingLink> subsPlumbingLinks = [.. subsEntities
            .SelectMany(s => new[] { s.Number, s.Number2 }
                .Where(num => plumbingDict.ContainsKey(num))
                .Select(num => (SubsId: s.Id, PlumbingId: plumbingDict[num].Id, MatchingNumber: num))
            )
            .Where(pair => !existingSubsPlumbing.Contains((pair.SubsId, pair.PlumbingId)))
            .Select(pair => new SubsPlumbingLink
            {
                SubsId = pair.SubsId,
                SubsEntity = subsEntities.First(s => s.Id == pair.SubsId),
                PlumbingId = pair.PlumbingId,
                PlumbingEntity = plumbingDict[pair.PlumbingId],
                MatchingSubPhone = pair.MatchingNumber
            })];

        // Generate Subs-Call links
        List<CallSubsLink> subsCallLinks = [.. subsEntities
            .SelectMany(s => new[] { s.Number, s.Number2 }
                .Where(num => callDict.ContainsKey(num))
                .Select(num => (SubsId: s.Id, CallId: callDict[num].Id, MatchingNumber: num))
            )
            .Where(pair => !existingSubsCall.Contains((pair.SubsId, pair.CallId)))
            .Select(pair => new CallSubsLink
            {
                SubsId = pair.SubsId,
                SubsEntity = subsEntities.First(s => s.Id == pair.SubsId),
                CallId = pair.CallId,
                CallEntity = callDict[pair.CallId],
                MatchingNumber = pair.MatchingNumber
            })];

        // Generate Plumbing-Call links
        List<PlumbingCallLink> plumbingCallLinks = [.. plumbingEntities
            .Where(p => callDict.ContainsKey(p.PhoneNumber))
            .Select(p => (PlumbingId: p.Id, CallId: callDict[p.PhoneNumber].Id))
            .Where(pair => !existingPlumbingCall.Contains((pair.PlumbingId, pair.CallId)))
            .Select(pair => new PlumbingCallLink
            {
                PlumbingId = pair.PlumbingId,
                PlumbingEntity = plumbingEntities.First(p => p.Id == pair.PlumbingId),
                CallId = pair.CallId,
                CallEntity = callDict[pair.CallId]
            })];

        // Save links to DB
        Result<List<SubsPlumbingLink>> addedSubsPlumbingLinks = await _subsPlumbingRepo.UpsertRangeAsync(subsPlumbingLinks);
        Result<List<CallSubsLink>> addedSubsCallLinks = await _subsCallRepo.UpsertRangeAsync(subsCallLinks);
        Result<List<PlumbingCallLink>> addedPlumbingCallLinks = await _plumbingCallRepo.UpsertRangeAsync(plumbingCallLinks);

        // Combine all results
        return Result.Combine(addedSubsPlumbingLinks, addedSubsCallLinks, addedPlumbingCallLinks);
    }
}
