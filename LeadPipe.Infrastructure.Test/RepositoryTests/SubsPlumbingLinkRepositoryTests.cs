using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Repository;
using CSharpFunctionalExtensions;

namespace LeadPipe.Infrastructure.Test.RepositoryTests;

public class SubsPlumbingLinkRepositoryTests
{

    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleEntities()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var repo = new SubsPlumbingLinkRepository(context);

        var entities = new List<SubsPlumbingLink>
        {
            new() { Id = 1, SubsEntity = new(), PlumbingEntity = new() },
            new() { Id = 2, SubsEntity = new(), PlumbingEntity = new() }
        };

        var result = await repo.AddRangeAsync(entities);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, context.SubsPlumbingLinks.Count());
    }

    [Fact]
    public async Task AddRangeAsync_ShouldFail_WhenEmptyList()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var repo = new SubsPlumbingLinkRepository(context);

        var result = await repo.AddRangeAsync([]);

        Assert.False(result.IsSuccess);
        Assert.Contains("No plumbing entities", result.Error);
    }

    [Fact]
    public async Task AddAsync_ShouldAddSubsPlumbingLink()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var repo = new SubsPlumbingLinkRepository(context);

        var plumbing = new SubsPlumbingLink { Id = 1, SubsEntity = new(), PlumbingEntity = new() };
        Result result = await repo.AddAsync(plumbing);

        Assert.True(result.IsSuccess);
    }
    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        context.SubsPlumbingLinks.Add(new SubsPlumbingLink { Id = 1, SubsEntity = new(), PlumbingEntity = new() });
        await context.SaveChangesAsync();

        var repo = new SubsPlumbingLinkRepository(context);
        var result = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Id);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var plumbing = new SubsPlumbingLink { Id = 1, SubsEntity = new(), PlumbingEntity = new() };
        context.SubsPlumbingLinks.Add(plumbing);
        await context.SaveChangesAsync();

        var repo = new SubsPlumbingLinkRepository(context);
        var result = await repo.DeleteAsync(1);
        var reloaded = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.True(reloaded.IsFailure);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldFail_WhenNotFound()
    {
        var repo = new SubsPlumbingLinkRepository(RepoTestHelpers.GetInMemoryContext());
        var result = await repo.GetByIdAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldUpdateEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var plumbing = new SubsPlumbingLink { Id = 1, SubsEntity = new(), PlumbingEntity = new() };
        context.SubsPlumbingLinks.Add(plumbing);
        await context.SaveChangesAsync();

        var repo = new SubsPlumbingLinkRepository(context);
        var updatedSubsPlumbingLink = new SubsPlumbingLink { Id = 1, SubsEntity = new(), PlumbingEntity = new() };

        var result = await repo.UpdateAsync(updatedSubsPlumbingLink);
        var reloaded = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.True(reloaded.IsSuccess);
        Assert.Equal(1, reloaded.Value.Id);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldFail_WhenEntityDoesNotExist()
    {
        var repo = new SubsPlumbingLinkRepository(RepoTestHelpers.GetInMemoryContext());
        var updatedSubsPlumbingLink = new SubsPlumbingLink { Id = 99, SubsEntity = new(), PlumbingEntity = new() };

        var result = await repo.UpdateAsync(updatedSubsPlumbingLink);

        Assert.False(result.IsSuccess);
        Assert.Contains("does not exist", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSucceed_WhenEntityDoesNotExist()
    {
        var repo = new SubsPlumbingLinkRepository(RepoTestHelpers.GetInMemoryContext());
        var result = await repo.DeleteAsync(99);

        Assert.True(result.IsSuccess);
    }
}
