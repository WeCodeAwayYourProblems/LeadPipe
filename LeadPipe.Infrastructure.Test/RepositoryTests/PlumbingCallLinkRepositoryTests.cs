using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Sqlite.Repository;
using LeadPipe.Infrastructure.Entity.Sqlite;

namespace LeadPipe.Infrastructure.Test.RepositoryTests;

public class PlumbingCallLinkRepositoryTests
{

    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleEntities()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var repo = new PlumbingCallLinkRepository(context);

        var entities = new List<PlumbingCallLink>
        {
            new() { Id = 1, PlumbingEntity = new(), CallEntity = new(){Note = string.Empty, Source= string.Empty} },
            new() { Id = 2, PlumbingEntity = new(), CallEntity = new(){Note = string.Empty, Source= string.Empty } }
        };

        var result = await repo.AddRangeAsync(entities);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, context.PlumbingCallLinks.Count());
    }

    [Fact]
    public async Task AddRangeAsync_ShouldFail_WhenEmptyList()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var repo = new PlumbingCallLinkRepository(context);

        var result = await repo.AddRangeAsync([]);

        Assert.False(result.IsSuccess);
        Assert.Contains("No plumbing entities", result.Error);
    }

    [Fact]
    public async Task AddAsync_ShouldAddPlumbingCallLink()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var repo = new PlumbingCallLinkRepository(context);

        var plumbing = new PlumbingCallLink { Id = 1, PlumbingEntity = new(), CallEntity = new() { Note = string.Empty, Source = string.Empty } };
        Result result = await repo.AddAsync(plumbing);

        Assert.True(result.IsSuccess);
    }
    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        context.PlumbingCallLinks.Add(new PlumbingCallLink { Id = 1, PlumbingEntity = new(), CallEntity = new() { Note = string.Empty, Source = string.Empty } });
        await context.SaveChangesAsync();

        var repo = new PlumbingCallLinkRepository(context);
        var result = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Id);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var plumbing = new PlumbingCallLink { Id = 1, PlumbingEntity = new(), CallEntity = new() { Note = string.Empty, Source = string.Empty } };
        context.PlumbingCallLinks.Add(plumbing);
        await context.SaveChangesAsync();

        var repo = new PlumbingCallLinkRepository(context);
        var result = await repo.DeleteAsync(1);
        var reloaded = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.True(reloaded.IsFailure);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldFail_WhenNotFound()
    {
        var repo = new PlumbingCallLinkRepository(RepoTestHelpers.GetInMemoryContext());
        var result = await repo.GetByIdAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldUpdateEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var plumbing = new PlumbingCallLink { Id = 1, PlumbingEntity = new(), CallEntity = new() { Note = string.Empty, Source = string.Empty } };
        context.PlumbingCallLinks.Add(plumbing);
        await context.SaveChangesAsync();

        var repo = new PlumbingCallLinkRepository(context);
        var updatedPlumbingCallLink = new PlumbingCallLink { Id = 1, PlumbingEntity = new(), CallEntity = new() { Note = string.Empty, Source = string.Empty } };

        var result = await repo.UpdateAsync(updatedPlumbingCallLink);
        var reloaded = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.True(reloaded.IsSuccess);
        Assert.Equal(1, reloaded.Value.Id);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldFail_WhenEntityDoesNotExist()
    {
        var repo = new PlumbingCallLinkRepository(RepoTestHelpers.GetInMemoryContext());
        var updatedPlumbingCallLink = new PlumbingCallLink { Id = 99, PlumbingEntity = new(), CallEntity = new() { Note = string.Empty, Source = string.Empty } };

        var result = await repo.UpdateAsync(updatedPlumbingCallLink);

        Assert.False(result.IsSuccess);
        Assert.Contains("does not exist", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSucceed_WhenEntityDoesNotExist()
    {
        var repo = new PlumbingCallLinkRepository(RepoTestHelpers.GetInMemoryContext());
        var result = await repo.DeleteAsync(99);

        Assert.True(result.IsSuccess);
    }
}