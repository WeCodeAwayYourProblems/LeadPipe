using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Service;

internal class PlumbingAssociationService(
#region Ctor Params
    IPlumbingRepository plumbingRepo,
    ISubsRepository subsRepo,
    ICallRepository callRepo,
    ISubsPlumbingLinkRepository linkRepo,
    ISubsCallLinkRepository subsCallRepo,
    IPlumbingCallLinkRepository plumbingCallRepo,
    IVoToEntity<Plumbing, PlumbingEntity> plumbToEntity,
    IVoToEntity<Call, CallEntity> callToEntity,
    IVoToEntity<Sandwich, SubsEntity> sandToEntity,
    IEntityToVo<PlumbingEntity, Plumbing> entityToPlumb,
    IEntityToVo<SubsEntity, Sandwich> entityToSand,
    IEntityToVo<CallEntity, Call> entityToCall
#endregion
    ) : IPlumbingAssociationService
{
    #region Private
    private readonly IPlumbingRepository _plumbingRepo = plumbingRepo;
    private readonly ISubsRepository _subsRepo = subsRepo;
    private readonly ICallRepository _callRepo = callRepo;

    private readonly ISubsPlumbingLinkRepository _subsPlumbingRepo = linkRepo;
    private readonly ISubsCallLinkRepository _subsCallRepo = subsCallRepo;
    private readonly IPlumbingCallLinkRepository _plumbingCallRepo = plumbingCallRepo;

    private readonly IVoToEntity<Plumbing, PlumbingEntity> _plumbToEntity = plumbToEntity;
    private readonly IVoToEntity<Call, CallEntity> _callToEntity = callToEntity;
    private readonly IVoToEntity<Sandwich, SubsEntity> _sandToEntity = sandToEntity;

    private readonly IEntityToVo<PlumbingEntity, Plumbing> _entityToPlumb = entityToPlumb;
    private readonly IEntityToVo<SubsEntity, Sandwich> _entityToSand = entityToSand;
    private readonly IEntityToVo<CallEntity, Call> _entityToCall = entityToCall;
    #endregion

    public async Task<Result<List<Plumbing>>> GetPlumbingAsync()
    {
        Result<List<PlumbingEntity>> entitiesResult = await _plumbingRepo.GetAllAsync();
        if (entitiesResult.IsFailure)
            return Result.Failure<List<Plumbing>>(entitiesResult.Error);

        List<Plumbing> voList = [.. entitiesResult.Value.Select(_entityToPlumb.Translate)];
        return Result.Success(voList);
    }

    public async Task<Result<List<Sandwich>>> GetSandwichAsync()
    {
        Result<List<SubsEntity>> entitiesResult = await _subsRepo.GetAllAsync();
        if (entitiesResult.IsFailure)
            return Result.Failure<List<Sandwich>>(entitiesResult.Error);

        List<Sandwich> voList = [.. entitiesResult.Value.Select(_entityToSand.Translate)];
        return Result.Success(voList);
    }

    public async Task<Result<List<Call>>> GetCallAsync()
    {
        Result<List<CallEntity>> entitiesResult = await _callRepo.GetAllAsync();
        if (entitiesResult.IsFailure)
            return Result.Failure<List<Call>>(entitiesResult.Error);

        List<Call> voList = [.. entitiesResult.Value.Select(_entityToCall.Translate)];
        return Result.Success(voList);
    }

    public async Task<Result> SaveAllAsync(List<Plumbing> plumb, List<Sandwich> subs, List<Call> calls)
    {
        // Convert VOs to Entities
        List<PlumbingEntity> plumbingEntities = [.. plumb.Select(_plumbToEntity.Translate)];
        List<SubsEntity> subsEntities = [.. subs.Select(_sandToEntity.Translate)];
        List<CallEntity> callEntities = [.. calls.Select(_callToEntity.Translate)];

        // Batch insert Plumbing, Subs, and Calls
        await _plumbingRepo.AddRangeAsync(plumbingEntities);
        await _subsRepo.AddRangeAsync(subsEntities);
        await _callRepo.AddRangeAsync(callEntities);

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
        Result<List<SubsPlumbingLink>> addedSubsPlumbingLinks = await _subsPlumbingRepo.AddRangeAsync(subsPlumbingLinks);
        Result<List<CallSubsLink>> addedSubsCallLinks = await _subsCallRepo.AddRangeAsync(subsCallLinks);
        Result<List<PlumbingCallLink>> addedPlumbingCallLinks = await _plumbingCallRepo.AddRangeAsync(plumbingCallLinks);

        // Combine all results
        return Result.Combine(addedSubsPlumbingLinks, addedSubsCallLinks, addedPlumbingCallLinks);
    }
}
