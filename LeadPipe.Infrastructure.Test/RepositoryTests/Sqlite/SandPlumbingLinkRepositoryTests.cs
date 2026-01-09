using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Repository;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Test.RepositoryTests.Sqlite;

public class SandPlumbingLinkRepositoryTests
{
    private readonly ILogger<SandPlumbingLinkRepository> logger = LoggerFactory
        .Create(builder => builder.AddConsole())
        .CreateLogger<SandPlumbingLinkRepository>();
    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleEntities()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        
        var repo = new SandPlumbingLinkRepository(context, logger);

        var entities = new List<SandPlumbingLink>
        {
            new() { Id = 1, SandEntity = new() { Id = 0  }, PlumbingEntity = new() { Id=0,MetaData = string.Empty } },
            new() { Id = 2, SandEntity = new() { Id = 0 }, PlumbingEntity = new() { Id=0,MetaData = string.Empty } }
        };

        var result = await repo.AddRangeAsync(entities);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, context.SandPlumbingLinks.Count());
    }

    [Fact]
    public async Task AddRangeAsync_ShouldFail_WhenEmptyList()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        
        var repo = new SandPlumbingLinkRepository(context, logger);

        var result = await repo.AddRangeAsync([]);

        Assert.False(result.IsSuccess);
        Assert.Contains("No plumbing entities", result.Error);
    }

    [Fact]
    public async Task AddAsync_ShouldAddSubsPlumbingLink()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        
        var repo = new SandPlumbingLinkRepository(context, logger);

        var plumbing = new SandPlumbingLink { Id = 1, SandEntity = new() { Id = 0 }, PlumbingEntity = new() { Id = 0, MetaData = string.Empty } };
        Result result = await repo.AddAsync(plumbing);

        Assert.True(result.IsSuccess);
    }
    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        context.SandPlumbingLinks.Add(new SandPlumbingLink { Id = 1, SandEntity = new() { Id = 0 }, PlumbingEntity = new() { Id = 0, MetaData = string.Empty } });
        await context.SaveChangesAsync();

        var repo = new SandPlumbingLinkRepository(context, logger);
        var result = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Id);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var plumbing = new SandPlumbingLink { Id = 1, SandEntity = new() { Id = 0 }, PlumbingEntity = new() { Id = 0, MetaData = string.Empty } };
        context.SandPlumbingLinks.Add(plumbing);
        await context.SaveChangesAsync();
        
        var repo = new SandPlumbingLinkRepository(context, logger);
        var result = await repo.DeleteAsync(1);
        var reloaded = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.True(reloaded.IsFailure);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldFail_WhenNotFound()
    {
        var repo = new SandPlumbingLinkRepository(RepoTestHelpers.GetInMemoryContext(), logger);
        var result = await repo.GetByIdAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldUpdateEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var plumbing = new SandPlumbingLink { Id = 1, SandEntity = new() { Id = 0 }, PlumbingEntity = new() { Id = 0, MetaData = string.Empty } };
        context.SandPlumbingLinks.Add(plumbing);
        await context.SaveChangesAsync();
        
        var repo = new SandPlumbingLinkRepository(context, logger);
        var updatedSubsPlumbingLink = new SandPlumbingLink { Id = 1, SandEntity = new() { Id = 0 }, PlumbingEntity = new() { Id = 0, MetaData = string.Empty } };

        var result = await repo.UpdateAsync(updatedSubsPlumbingLink);
        var reloaded = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.True(reloaded.IsSuccess);
        Assert.Equal(1, reloaded.Value.Id);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldFail_WhenEntityDoesNotExist()
    {
        var repo = new SandPlumbingLinkRepository(RepoTestHelpers.GetInMemoryContext(), logger);
        var updatedSubsPlumbingLink = new SandPlumbingLink { Id = 99, SandEntity = new() { Id = 0 }, PlumbingEntity = new() { Id = 0, MetaData = string.Empty } };

        var result = await repo.UpdateAsync(updatedSubsPlumbingLink);

        Assert.False(result.IsSuccess);
        Assert.Contains("does not exist", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSucceed_WhenEntityDoesNotExist()
    {
        var repo = new SandPlumbingLinkRepository(RepoTestHelpers.GetInMemoryContext(), logger);
        var result = await repo.DeleteAsync(99);

        Assert.True(result.IsSuccess);
    }
}
