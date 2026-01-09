using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Repository;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Test.RepositoryTests.Sqlite;

public class PlumbingCallLinkRepositoryTests
{
    private readonly ILogger<PlumbingCallLinkRepository> logger = LoggerFactory
        .Create(builder => builder.AddConsole())
        .CreateLogger<PlumbingCallLinkRepository>();

    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleEntities()
    {
        var context = RepoTestHelpers.GetInMemoryContext();

        var repo = new PlumbingCallLinkRepository(context, logger);

        var entities = new List<PlumbingCallLink>
        {
            new() { Id = 1, PlumbingEntity = new(){ Id = 0, MetaData=string.Empty }, CallEntity = new(){ Id=0, Note = string.Empty, Location = string.Empty, Source= string.Empty} },
            new() { Id = 2, PlumbingEntity = new(){ Id = 0, MetaData=string.Empty }, CallEntity = new(){ Id=0, Note = string.Empty, Location = string.Empty, Source= string.Empty } }
        };

        var result = await repo.AddRangeAsync(entities);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, context.PlumbingCallLinks.Count());
    }

    [Fact]
    public async Task AddRangeAsync_ShouldFail_WhenEmptyList()
    {
        var context = RepoTestHelpers.GetInMemoryContext();

        var repo = new PlumbingCallLinkRepository(context, logger);

        var result = await repo.AddRangeAsync([]);

        Assert.False(result.IsSuccess);
        Assert.Contains("No plumbing entities", result.Error);
    }

    [Fact]
    public async Task AddAsync_ShouldAddPlumbingCallLink()
    {
        var context = RepoTestHelpers.GetInMemoryContext();

        var repo = new PlumbingCallLinkRepository(context, logger);

        var plumbing = new PlumbingCallLink { Id = 1, PlumbingEntity = new() { Id = 0, MetaData = string.Empty }, CallEntity = new() { Id = 0, Note = string.Empty, Location = string.Empty, Source = string.Empty } };
        Result result = await repo.AddAsync(plumbing);

        Assert.True(result.IsSuccess);
    }
    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        context.PlumbingCallLinks.Add(new PlumbingCallLink { Id = 1, PlumbingEntity = new() { Id = 0, MetaData = string.Empty }, CallEntity = new() { Id = 0, Note = string.Empty, Location = string.Empty, Source = string.Empty } });
        await context.SaveChangesAsync();

        var logger = LoggerFactory
            .Create(builder => builder.AddConsole())
            .CreateLogger<PlumbingCallLinkRepository>();

        var repo = new PlumbingCallLinkRepository(context, logger);
        var result = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Id);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var plumbing = new PlumbingCallLink { Id = 1, PlumbingEntity = new() { Id = 0, MetaData = string.Empty }, CallEntity = new() { Id = 0, Note = string.Empty, Location = string.Empty, Source = string.Empty } };
        context.PlumbingCallLinks.Add(plumbing);
        await context.SaveChangesAsync();


        var repo = new PlumbingCallLinkRepository(context, logger);
        var result = await repo.DeleteAsync(1);
        var reloaded = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.True(reloaded.IsFailure);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldFail_WhenNotFound()
    {

        var repo = new PlumbingCallLinkRepository(RepoTestHelpers.GetInMemoryContext(), logger);
        var result = await repo.GetByIdAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldUpdateEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var plumbing = new PlumbingCallLink { Id = 1, PlumbingEntity = new() { Id = 0, MetaData = string.Empty }, CallEntity = new() { Id = 0, Note = string.Empty, Location = string.Empty, Source = string.Empty } };
        context.PlumbingCallLinks.Add(plumbing);
        await context.SaveChangesAsync();

        var repo = new PlumbingCallLinkRepository(context, logger);
        var updatedPlumbingCallLink = new PlumbingCallLink { Id = 1, PlumbingEntity = new() { Id = 0, MetaData = string.Empty }, CallEntity = new() { Id = 0, Note = string.Empty, Location = string.Empty, Source = string.Empty } };

        var result = await repo.UpdateAsync(updatedPlumbingCallLink);
        var reloaded = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.True(reloaded.IsSuccess);
        Assert.Equal(1, reloaded.Value.Id);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldFail_WhenEntityDoesNotExist()
    {
        var repo = new PlumbingCallLinkRepository(RepoTestHelpers.GetInMemoryContext(), logger);
        var updatedPlumbingCallLink = new PlumbingCallLink { Id = 99, PlumbingEntity = new() { Id = 0, MetaData = string.Empty }, CallEntity = new() { Id = 0, Note = string.Empty, Location = string.Empty, Source = string.Empty } };

        var result = await repo.UpdateAsync(updatedPlumbingCallLink);

        Assert.False(result.IsSuccess);
        Assert.Contains("does not exist", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSucceed_WhenEntityDoesNotExist()
    {
        var repo = new PlumbingCallLinkRepository(RepoTestHelpers.GetInMemoryContext(), logger);
        var result = await repo.DeleteAsync(99);

        Assert.True(result.IsSuccess);
    }
}
