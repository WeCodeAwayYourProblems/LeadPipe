using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Data.Load;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Repository;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Test.RepositoryTests.Sqlite;

public class SubsCaliperLinkRepositoryTests
{
    private readonly ILogger<SubsCaliperLinkRepository> logger = LoggerFactory
        .Create(builder => builder.AddConsole())
        .CreateLogger<SubsCaliperLinkRepository>();
    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleEntities()
    {
        var context = RepoTestHelpers.GetInMemoryContext();

        var repo = new SubsCaliperLinkRepository(context, logger);

        var entities = new List<SandCaliperLink>
        {
            new() { Id = 1, SandEntity = new() { Id = 0 }, CaliperEntity = new() { Id = 0, Note=string.Empty, Location = string.Empty, Source=string.Empty } },
            new() { Id = 2, SandEntity = new() { Id = 0 }, CaliperEntity = new() { Id = 0, Note=string.Empty, Location = string.Empty, Source=string.Empty } }
        };

        var result = await repo.AddRangeAsync(entities);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, context.SandCaliperLinks.Count());
    }

    [Fact]
    public async Task AddRangeAsync_ShouldFail_WhenEmptyList()
    {
        var context = RepoTestHelpers.GetInMemoryContext();

        var repo = new SubsCaliperLinkRepository(context, logger);

        var result = await repo.AddRangeAsync([]);

        Assert.False(result.IsSuccess);
        Assert.Contains("No plumbing entities", result.Error);
    }

    [Fact]
    public async Task AddAsync_ShouldAddSubsCaliperLink()
    {
        var context = RepoTestHelpers.GetInMemoryContext();

        var repo = new SubsCaliperLinkRepository(context, logger);

        var plumbing = new SandCaliperLink { Id = 1, SandEntity = new() { Id = 0 }, CaliperEntity = new() { Id = 0, Note = string.Empty, Location = string.Empty, Source = string.Empty } };
        Result result = await repo.AddAsync(plumbing);

        Assert.True(result.IsSuccess);
    }
    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        context.SandCaliperLinks.Add(new SandCaliperLink { Id = 1, SandEntity = new() { Id = 0 }, CaliperEntity = new() { Id = 0, Note = string.Empty, Location = string.Empty, Source = string.Empty } });
        await context.SaveChangesAsync();

        var repo = new SubsCaliperLinkRepository(context, logger);
        var result = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Id);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var plumbing = new SandCaliperLink { Id = 1, SandEntity = new() { Id = 0 }, CaliperEntity = new() { Id = 0, Note = string.Empty, Location = string.Empty, Source = string.Empty } };
        context.SandCaliperLinks.Add(plumbing);
        await context.SaveChangesAsync();

        var repo = new SubsCaliperLinkRepository(context, logger);
        var result = await repo.DeleteAsync(1);
        var reloaded = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.True(reloaded.IsFailure);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldFail_WhenNotFound()
    {
        var repo = new SubsCaliperLinkRepository(RepoTestHelpers.GetInMemoryContext(), logger);
        var result = await repo.GetByIdAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldUpdateEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var plumbing = new SandCaliperLink { Id = 1, SandEntity = new() { Id = 0 }, CaliperEntity = new() { Id = 0, Note = string.Empty, Location = string.Empty, Source = string.Empty } };
        context.SandCaliperLinks.Add(plumbing);
        await context.SaveChangesAsync();

        var repo = new SubsCaliperLinkRepository(context, logger);
        var updatedSubsCaliperLink = new SandCaliperLink { Id = 1, SandEntity = new() { Id = 0 }, CaliperEntity = new() { Id = 0, Note = string.Empty, Location = string.Empty, Source = string.Empty } };

        var result = await repo.UpdateAsync(updatedSubsCaliperLink);
        var reloaded = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.True(reloaded.IsSuccess);
        Assert.Equal(1, reloaded.Value.Id);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldFail_WhenEntityDoesNotExist()
    {
        var repo = new SubsCaliperLinkRepository(RepoTestHelpers.GetInMemoryContext(), logger);
        var updatedSubsCaliperLink = new SandCaliperLink { Id = 99, SandEntity = new() { Id = 0 }, CaliperEntity = new() { Id = 0, Note = string.Empty, Location = string.Empty, Source = string.Empty } };

        var result = await repo.UpdateAsync(updatedSubsCaliperLink);

        Assert.False(result.IsSuccess);
        Assert.Contains("does not exist", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSucceed_WhenEntityDoesNotExist()
    {
        var repo = new SubsCaliperLinkRepository(RepoTestHelpers.GetInMemoryContext(), logger);
        var result = await repo.DeleteAsync(99);

        Assert.True(result.IsSuccess);
    }
}
