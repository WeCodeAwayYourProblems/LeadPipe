using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Repository;
using LeadPipe.Infrastructure.Translate;

namespace LeadPipe.Infrastructure.Service;

internal class PlumbingAssociationService(
    IPlumbingRepository plumbingRepo,
    ISubsRepository subsRepo,
    ICallRepository callRepo,
    ISubsPlumbingLinkRepository linkRepo,
    ISubsCallLinkRepository subsCallRepo,
    IPlumbingCallLinkRepository plumbingCallRepo,
    IVoToEntity voToEntity,
    IEntityToVo entityToVo) : IPlumbingAssociationService
{
    private readonly IPlumbingRepository _plumbingRepo = plumbingRepo;
    private readonly ISubsRepository _subsRepo = subsRepo;
    private readonly ICallRepository _callRepo = callRepo;

    private readonly ISubsPlumbingLinkRepository _subsPlumbingRepo = linkRepo;
    private readonly ISubsCallLinkRepository _subsCallRepo = subsCallRepo;
    private readonly IPlumbingCallLinkRepository _plumbingCallRepo = plumbingCallRepo;

    private readonly IVoToEntity _voToEntity = voToEntity;
    private readonly IEntityToVo _entityToVo = entityToVo;

    public async Task<Result<List<Plumbing>>> GetPlumbingAsync()
    {
        Result<List<PlumbingEntity>> entitiesResult = await _plumbingRepo.GetAllAsync();
        if (entitiesResult.IsFailure)
            return Result.Failure<List<Plumbing>>(entitiesResult.Error);

        List<Plumbing> voList = [.. entitiesResult.Value.Select(_entityToVo.Translate)];
        return Result.Success(voList);
    }

    public async Task<Result<List<Sandwich>>> GetSandwichAsync()
    {
        Result<List<SubsEntity>> entitiesResult = await _subsRepo.GetAllAsync();
        if (entitiesResult.IsFailure)
            return Result.Failure<List<Sandwich>>(entitiesResult.Error);

        List<Sandwich> voList = [.. entitiesResult.Value.Select(_entityToVo.Translate)];
        return Result.Success(voList);
    }

    public async Task<Result<List<Call>>> GetCallAsync()
    {
        Result<List<CallEntity>> entitiesResult = await _callRepo.GetAllAsync();
        if (entitiesResult.IsFailure)
            return Result.Failure<List<Call>>(entitiesResult.Error);

        List<Call> voList = [.. entitiesResult.Value.Select(_entityToVo.Translate)];
        return Result.Success(voList);
    }

    public async Task<Result> SaveAllAsync(List<Plumbing> plumb, List<Sandwich> subs, List<Call> calls)
    {
        List<PlumbingEntity> plumbingEntities = [.. plumb.Select(_voToEntity.Translate)];
        List<SubsEntity> subsEntities = [.. subs.Select(_voToEntity.Translate)];
        List<CallEntity> callEntities = [.. calls.Select(_voToEntity.Translate)];

        // Batch insert PlumbingEntities and SubsEntities
        await _plumbingRepo.AddRangeAsync(plumbingEntities);
        await _subsRepo.AddRangeAsync(subsEntities);

        // Generate links using LINQ
        Dictionary<long, PlumbingEntity> plumbingDict = plumbingEntities.ToDictionary(p => p.PhoneNumber);
        List<SubsPlumbingLink> subsPlumbingLinks = subsEntities
            .SelectMany(s => new[] { s.Number, s.Number2 }
                .Where(num => plumbingDict.ContainsKey(num))
                .Select(num => new SubsPlumbingLink
                {
                    SubsId = s.Id,
                    SubsEntity = s,
                    MatchingSubPhone = num,
                    PlumbingId = plumbingDict[num].Id,
                    PlumbingEntity = plumbingDict[num]
                })
            ).ToList();

        Dictionary<long, CallEntity> pcallDict = callEntities.ToDictionary(c => c.PhoneNumber);
        List<PlumbingCallLink> plumbingCallLink = plumbingEntities
            .Where(p => pcallDict.ContainsKey(p.PhoneNumber))
            .Select(p => new PlumbingCallLink
            {
                PlumbingId = p.Id,
                PlumbingEntity = p,
                CallId = pcallDict[p.PhoneNumber].Id,
                CallEntity = pcallDict[p.PhoneNumber]
            }).ToList();

        Dictionary<long, CallEntity> callDict = callEntities.ToDictionary(c => c.PhoneNumber, c => c);
        List<SubsCallLink> subsCallLink = subsEntities
            .SelectMany(s => new[] { s.Number, s.Number2 }
                .Where(num => callDict.ContainsKey(num))
                .Select(num => new SubsCallLink
                {
                    SubsId = s.Id,
                    SubsEntity = s,
                    CallId = callDict[num].Id,
                    CallEntity = callDict[num],
                    MatchingNumber = num
                })
            ).ToList();

        Result<List<SubsPlumbingLink>> addedSubsPlumbingLinks = await _subsPlumbingRepo.AddRangeAsync(subsPlumbingLinks);
        Result<List<SubsCallLink>> addedSubsCallLinks = await _subsCallRepo.AddRangeAsync(subsCallLink);
        Result<List<PlumbingCallLink>> addedPlumbingCallLinks = await _plumbingCallRepo.AddRangeAsync(plumbingCallLink);

        // Verify all links exist
        Result<List<SubsPlumbingLink>> subPlumbLinksResult = await _subsPlumbingRepo.GetAllAsync();
        Result<List<SubsCallLink>> subCallLinksResult = await _subsCallRepo.GetAllAsync();
        Result<List<PlumbingCallLink>> plumbCallLinksResult = await _plumbingCallRepo.GetAllAsync();

        bool subsPlumbLinksExist = subPlumbLinksResult.IsSuccess && subsPlumbingLinks.All(l => subPlumbLinksResult.Value.Any(dbLink =>
            dbLink.SubsId == l.SubsId && dbLink.PlumbingId == l.PlumbingId));
        bool subsCallLinksExist = subCallLinksResult.IsSuccess && subsCallLink.All(l => subCallLinksResult.Value.Any(link =>
            link.SubsId == l.SubsId && link.CallId == l.CallId));
        bool plumbCallLinksExist = plumbCallLinksResult.IsSuccess && plumbingCallLink.All(l => plumbCallLinksResult.Value.Any(link =>
            link.PlumbingId == l.PlumbingId && link.CallId == l.CallId));

        return Result.Combine(subPlumbLinksResult, subCallLinksResult, plumbCallLinksResult, addedSubsPlumbingLinks, addedSubsCallLinks, addedPlumbingCallLinks);
    }
}