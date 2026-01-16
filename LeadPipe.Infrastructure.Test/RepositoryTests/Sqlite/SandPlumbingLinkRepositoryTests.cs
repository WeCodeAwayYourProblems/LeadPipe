using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Repository;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Test.RepositoryTests.Sqlite;

public class SandPlumbingLinkRepositoryTests
{
    private readonly SandEntity _sandy = new() { Id = 0, CustardId = 0, Offerman = string.Empty };
    private readonly PlumbingEntity _plumb = new() { Id = 0, MetaData = string.Empty };
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
            new() { Id = 1, SandEntity = _sandy, PlumbingEntity = _plumb },
            new() { Id = 2, SandEntity = _sandy, PlumbingEntity = _plumb }
        };

        var result = await repo.UpsertRangeAsync(entities);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, context.SandPlumbingLinks.Count());
    }

    [Fact]
    public async Task AddRangeAsync_ShouldFail_WhenEmptyList()
    {
        var context = RepoTestHelpers.GetInMemoryContext();

        var repo = new SandPlumbingLinkRepository(context, logger);

        var result = await repo.UpsertRangeAsync([]);

        Assert.False(result.IsSuccess);
        Assert.Contains("No entities", result.Error);
    }

    [Fact]
    public async Task AddAsync_ShouldAddSubsPlumbingLink()
    {
        var context = RepoTestHelpers.GetInMemoryContext();

        var repo = new SandPlumbingLinkRepository(context, logger);

        var plumbing = new SandPlumbingLink { Id = 1, SandEntity = _sandy, PlumbingEntity = _plumb };
        Result result = await repo.UpsertRangeAsync([plumbing]);

        Assert.True(result.IsSuccess);
    }
    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        context.SandPlumbingLinks.Add(new SandPlumbingLink { Id = 1, SandEntity = _sandy, PlumbingEntity = _plumb });
        await context.SaveChangesAsync();

        var repo = new SandPlumbingLinkRepository(context, logger);
        var result = await repo.FindAsync(l => l.Id == 1);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value[0].Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldFail_WhenNotFound()
    {
        var repo = new SandPlumbingLinkRepository(RepoTestHelpers.GetInMemoryContext(), logger);
        var result = await repo.FindAsync(l => l.Id == 99);

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldUpdateEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var plumbing = new SandPlumbingLink { Id = 1, SandEntity = _sandy, PlumbingEntity = _plumb };
        context.SandPlumbingLinks.Add(plumbing);
        await context.SaveChangesAsync();

        var repo = new SandPlumbingLinkRepository(context, logger);
        var updatedSubsPlumbingLink = new SandPlumbingLink { Id = 1, SandEntity = _sandy, PlumbingEntity = _plumb };

        var result = await repo.UpsertRangeAsync([updatedSubsPlumbingLink]);
        var reloaded = await repo.FindAsync(l => l.Id == 1);

        Assert.True(result.IsSuccess);
        Assert.True(reloaded.IsSuccess);
        Assert.Equal(1, reloaded.Value[0].Id);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldFail_WhenEntityDoesNotExist()
    {
        var repo = new SandPlumbingLinkRepository(RepoTestHelpers.GetInMemoryContext(), logger);
        var updatedSubsPlumbingLink = new SandPlumbingLink { Id = 99, SandEntity = _sandy, PlumbingEntity = _plumb };

        var result = await repo.UpsertRangeAsync([updatedSubsPlumbingLink]);

        Assert.False(result.IsSuccess);
        Assert.Contains("does not exist", result.Error);
    }
}
