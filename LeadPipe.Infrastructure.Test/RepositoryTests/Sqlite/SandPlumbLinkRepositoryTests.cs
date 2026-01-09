using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Repository;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Test.RepositoryTests.Sqlite;

public class SandPlumbLinkRepositoryTests
{
    private readonly ILogger<SandPlumbingLinkRepository> logger = LoggerFactory
        .Create(builder => builder.AddConsole())
        .CreateLogger<SandPlumbingLinkRepository>();
    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleLinks()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        
        var repo = new SandPlumbingLinkRepository(context, logger);

        var links = new List<SandPlumbingLink>
        {
            new() { SandId = 1, SandEntity = new() { Id = 0 }, PlumbingId = 1, MatchingPhone = 12345, PlumbingEntity = new() { Id = 0, MetaData = string.Empty } },
            new() { SandId = 2, SandEntity = new() { Id = 0 }, PlumbingId = 2, MatchingPhone = 67890, PlumbingEntity = new() { Id = 0, MetaData = string.Empty } }
        };

        var result = await repo.AddRangeAsync(links);

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
        Assert.Contains("No link entities", result.Error);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldFail_WhenNullList()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        
        var repo = new SandPlumbingLinkRepository(context, logger);

        var result = await repo.AddRangeAsync([]);

        Assert.False(result.IsSuccess);
        Assert.Contains("No link entities", result.Error);
    }

    [Fact]
    public async Task AddAsync_ShouldAddLink()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        
        var repo = new SandPlumbingLinkRepository(context, logger);

        var link = new SandPlumbingLink { SandId = 1, SandEntity = new() { Id = 0 }, PlumbingId = 1, MatchingPhone = 12345, PlumbingEntity = new() { Id = 0, MetaData = string.Empty } };
        var result = await repo.AddAsync(link);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnLink_WhenExists()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        context.SandPlumbingLinks.Add(new SandPlumbingLink { SandId = 1, SandEntity = new() { Id = 0 }, PlumbingId = 1, MatchingPhone = 12345, PlumbingEntity = new() { Id = 0, MetaData = string.Empty } });
        await context.SaveChangesAsync();

        var repo = new SandPlumbingLinkRepository(context, logger);
        var result = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(12345, result.Value.MatchingPhone);
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
    public async Task UpdateValuesAsync_ShouldUpdateLink()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var link = new SandPlumbingLink { SandId = 1, SandEntity = new() { Id = 0 }, PlumbingId = 1, MatchingPhone = 12345, PlumbingEntity = new() { Id = 0, MetaData = string.Empty } };
        context.SandPlumbingLinks.Add(link);
        await context.SaveChangesAsync();

        var repo = new SandPlumbingLinkRepository(context, logger);
        var updatedLink = new SandPlumbingLink { SandId = 1, SandEntity = new() { Id = 0 }, PlumbingId = 1, MatchingPhone = 67890, PlumbingEntity = new() { Id = 0, MetaData = string.Empty } };

        var result = await repo.UpdateAsync(updatedLink);
        var reloaded = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.True(reloaded.IsSuccess);
        Assert.Equal(67890, reloaded.Value.MatchingPhone);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldFail_WhenEntityDoesNotExist()
    {
        var logger = LoggerFactory
            .Create(builder => builder.AddConsole())
            .CreateLogger<SandPlumbingLinkRepository>();

        var repo = new SandPlumbingLinkRepository(RepoTestHelpers.GetInMemoryContext(), logger);
        var updatedLink = new SandPlumbingLink { SandId = 99, SandEntity = new() { Id = 0 }, PlumbingId = 99, MatchingPhone = 11111, PlumbingEntity = new() { Id = 0, MetaData = string.Empty } };

        var result = await repo.UpdateAsync(updatedLink);

        Assert.False(result.IsSuccess);
        Assert.Contains("does not exist", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveLink()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var link = new SandPlumbingLink { SandId = 1, SandEntity = new() { Id = 0 }, PlumbingId = 1, MatchingPhone = 12345, PlumbingEntity = new() { Id = 0, MetaData = string.Empty } };
        context.SandPlumbingLinks.Add(link);
        await context.SaveChangesAsync();

        var repo = new SandPlumbingLinkRepository(context, logger);
        var result = await repo.DeleteAsync(1);
        var reloaded = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.True(reloaded.IsFailure);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSucceed_WhenEntityDoesNotExist()
    {
        var repo = new SandPlumbingLinkRepository(RepoTestHelpers.GetInMemoryContext(), logger);
        var result = await repo.DeleteAsync(99);

        Assert.True(result.IsSuccess);
    }
}
