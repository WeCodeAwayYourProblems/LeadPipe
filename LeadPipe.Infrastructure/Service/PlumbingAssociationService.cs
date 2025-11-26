using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Repository;
using LeadPipe.Infrastructure.Translate;

namespace LeadPipe.Infrastructure.Service;

internal class PlumbingAssociationService(
    IPlumbingRepository plumbingRepo,
    ISubsRepository subsRepo,
    ISubsPlumbingLinkRepository linkRepo,
    IVoToEntity voToEntity,
    IEntityToVo entityToVo) : IPlumbingAssociationService
{
    private readonly IPlumbingRepository _plumbingRepo = plumbingRepo;
    private readonly ISubsRepository _subsRepo = subsRepo;
    private readonly ISubsPlumbingLinkRepository _linkRepo = linkRepo;
    private readonly IVoToEntity _voToEntity = voToEntity;
    private readonly IEntityToVo _entityToVo = entityToVo;

    public async Task<Result<List<Plumbing>>> GetPlumbingAsync()
    {
        Result<List<PlumbingEntity>> entitiesResult = await _plumbingRepo.GetAllAsync();
        if (entitiesResult.IsFailure)
            return Result.Failure<List<Plumbing>>(entitiesResult.Error);

        List<Plumbing> voList = entitiesResult.Value.Select(_entityToVo.Translate).ToList();
        return Result.Success(voList);
    }

    public async Task<Result<List<Sandwich>>> GetSandwichAsync()
    {
        Result<List<SubsEntity>> entitiesResult = await _subsRepo.GetAllAsync();
        if (entitiesResult.IsFailure)
            return Result.Failure<List<Sandwich>>(entitiesResult.Error);

        List<Sandwich> voList = entitiesResult.Value.Select(_entityToVo.Translate).ToList();
        return Result.Success(voList);
    }

    public async Task<Result> SaveAllAsync(List<Plumbing> plumb, List<Sandwich> subs)
    {
        List<PlumbingEntity> plumbingEntities = [.. plumb.Select(_voToEntity.Translate)];
        List<SubsEntity> subsEntities = [.. subs.Select(_voToEntity.Translate)];

        // Batch insert PlumbingEntities and SubsEntities
        await _plumbingRepo.AddRangeAsync(plumbingEntities);
        await _subsRepo.AddRangeAsync(subsEntities);

        // Generate links using LINQ
        var links = (
            from s in subsEntities
            from p in plumbingEntities
            where p.PhoneNumber == s.Number || p.PhoneNumber == s.Number2
            select new SubsPlumbingLink
            {
                SubsId = s.Id,
                SubsEntity = s,
                MatchingSubPhone = p.PhoneNumber,
                PlumbingId = p.Id,
                PlumbingEntity = p
            }).ToList();

        if (links.Count == 0)
            return Result.Failure("No associations found between Plumbing and Subs entities.");

        await _linkRepo.AddRangeAsync(links);

        // Verify all links exist
        Result<List<SubsPlumbingLink>> allLinksResult = await _linkRepo.GetAllAsync();
        if (allLinksResult.IsFailure)
            return Result.Failure("Failed to retrieve links after save.");

        bool allLinksExist = links.All(l => allLinksResult.Value.Any(dbLink =>
            dbLink.SubsId == l.SubsId && dbLink.PlumbingId == l.PlumbingId));

        return allLinksExist ? Result.Success() : Result.Failure("Not all links were saved successfully.");
    }
}