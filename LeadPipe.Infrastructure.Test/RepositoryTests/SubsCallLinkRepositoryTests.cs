using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Repository;
using CSharpFunctionalExtensions;

namespace LeadPipe.Infrastructure.Test.RepositoryTests;

public class SubsCallLinkRepositoryTests
{

    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleEntities()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var repo = new SubsCallLinkRepository(context);

        var entities = new List<SubsCallLink>
        {
            new() { Id = 1, SubsEntity = new(), CallEntity = new() },
            new() { Id = 2, SubsEntity = new(), CallEntity = new() }
        };

        var result = await repo.AddRangeAsync(entities);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, context.SubsCallLinks.Count());
    }

    [Fact]
    public async Task AddRangeAsync_ShouldFail_WhenEmptyList()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var repo = new SubsCallLinkRepository(context);

        var result = await repo.AddRangeAsync([]);

        Assert.False(result.IsSuccess);
        Assert.Contains("No plumbing entities", result.Error);
    }

    [Fact]
    public async Task AddAsync_ShouldAddSubsCallLink()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var repo = new SubsCallLinkRepository(context);

        var plumbing = new SubsCallLink { Id = 1, SubsEntity = new(), CallEntity = new() };
        Result result = await repo.AddAsync(plumbing);

        Assert.True(result.IsSuccess);
    }
    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        context.SubsCallLinks.Add(new SubsCallLink { Id = 1, SubsEntity = new(), CallEntity = new() });
        await context.SaveChangesAsync();

        var repo = new SubsCallLinkRepository(context);
        var result = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Id);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var plumbing = new SubsCallLink { Id = 1, SubsEntity = new(), CallEntity = new() };
        context.SubsCallLinks.Add(plumbing);
        await context.SaveChangesAsync();

        var repo = new SubsCallLinkRepository(context);
        var result = await repo.DeleteAsync(1);
        var reloaded = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.True(reloaded.IsFailure);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldFail_WhenNotFound()
    {
        var repo = new SubsCallLinkRepository(RepoTestHelpers.GetInMemoryContext());
        var result = await repo.GetByIdAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldUpdateEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var plumbing = new SubsCallLink { Id = 1, SubsEntity = new(), CallEntity = new() };
        context.SubsCallLinks.Add(plumbing);
        await context.SaveChangesAsync();

        var repo = new SubsCallLinkRepository(context);
        var updatedSubsCallLink = new SubsCallLink { Id = 1, SubsEntity = new(), CallEntity = new() };

        var result = await repo.UpdateAsync(updatedSubsCallLink);
        var reloaded = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.True(reloaded.IsSuccess);
        Assert.Equal(1, reloaded.Value.Id);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldFail_WhenEntityDoesNotExist()
    {
        var repo = new SubsCallLinkRepository(RepoTestHelpers.GetInMemoryContext());
        var updatedSubsCallLink = new SubsCallLink { Id = 99, SubsEntity = new(), CallEntity = new() };

        var result = await repo.UpdateAsync(updatedSubsCallLink);

        Assert.False(result.IsSuccess);
        Assert.Contains("does not exist", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSucceed_WhenEntityDoesNotExist()
    {
        var repo = new SubsCallLinkRepository(RepoTestHelpers.GetInMemoryContext());
        var result = await repo.DeleteAsync(99);

        Assert.True(result.IsSuccess);
    }
}
