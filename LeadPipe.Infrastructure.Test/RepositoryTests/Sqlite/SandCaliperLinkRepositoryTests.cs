using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Data.Load;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Repository;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Test.RepositoryTests.Sqlite;

public class SubsCaliperLinkRepositoryTests
{
    private readonly SandEntity _sandy = new() { Id = 0, CustardId = 0, Offerman = string.Empty };
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
            new() { Id = 1, SandEntity = _sandy, CaliperEntity = new() { Id = 0, Note=string.Empty, Location = string.Empty, Source=string.Empty } },
            new() { Id = 2, SandEntity = _sandy, CaliperEntity = new() { Id = 0, Note=string.Empty, Location = string.Empty, Source=string.Empty } }
        };

        var result = await repo.UpsertRangeAsync(entities);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, context.SandCaliperLinks.Count());
    }

    [Fact]
    public async Task AddRangeAsync_ShouldFail_WhenEmptyList()
    {
        var context = RepoTestHelpers.GetInMemoryContext();

        var repo = new SubsCaliperLinkRepository(context, logger);

        var result = await repo.UpsertRangeAsync([]);

        Assert.False(result.IsSuccess);
        Assert.Contains("No entities", result.Error);
    }

    [Fact]
    public async Task AddAsync_ShouldAddSubsCaliperLink()
    {
        var context = RepoTestHelpers.GetInMemoryContext();

        var repo = new SubsCaliperLinkRepository(context, logger);

        var plumbing = new SandCaliperLink { Id = 1, SandEntity = new() { Id = 0, CustardId = 0, Offerman = string.Empty }, CaliperEntity = new() { Id = 0, Note = string.Empty, Location = string.Empty, Source = string.Empty } };
        Result result = await repo.UpsertRangeAsync([plumbing]);

        Assert.True(result.IsSuccess);
    }
    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        context.SandCaliperLinks.Add(new SandCaliperLink { Id = 1, SandEntity = new() { Id = 0, CustardId = 0, Offerman = string.Empty }, CaliperEntity = new() { Id = 0, Note = string.Empty, Location = string.Empty, Source = string.Empty } });
        await context.SaveChangesAsync();

        var repo = new SubsCaliperLinkRepository(context, logger);
        var result = await repo.FindAsync(l => l.Id == 1);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value[0].Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldFail_WhenNotFound()
    {
        var repo = new SubsCaliperLinkRepository(RepoTestHelpers.GetInMemoryContext(), logger);
        var result = await repo.FindAsync(l => l.Id == 99);

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldUpdateEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var plumbing = new SandCaliperLink { Id = 1, SandEntity = new() { Id = 0, CustardId = 0, Offerman = string.Empty }, CaliperEntity = new() { Id = 0, Note = string.Empty, Location = string.Empty, Source = string.Empty } };
        context.SandCaliperLinks.Add(plumbing);
        await context.SaveChangesAsync();

        var repo = new SubsCaliperLinkRepository(context, logger);
        var updatedSubsCaliperLink = new SandCaliperLink { Id = 1, SandEntity = new() { Id = 0, CustardId = 0, Offerman = string.Empty }, CaliperEntity = new() { Id = 0, Note = string.Empty, Location = string.Empty, Source = string.Empty } };

        var result = await repo.UpsertRangeAsync([updatedSubsCaliperLink]);
        var reloaded = await repo.FindAsync(l => l.Id == 1);

        Assert.True(result.IsSuccess);
        Assert.True(reloaded.IsSuccess);
        Assert.Equal(1, reloaded.Value[0].Id);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldFail_WhenEntityDoesNotExist()
    {
        var repo = new SubsCaliperLinkRepository(RepoTestHelpers.GetInMemoryContext(), logger);
        var updatedSubsCaliperLink = new SandCaliperLink { Id = 99, SandEntity = new() { Id = 0, CustardId = 0, Offerman = string.Empty }, CaliperEntity = new() { Id = 0, Note = string.Empty, Location = string.Empty, Source = string.Empty } };

        var result = await repo.UpsertRangeAsync([updatedSubsCaliperLink]);

        Assert.False(result.IsSuccess);
        Assert.Contains("does not exist", result.Error);
    }
}
