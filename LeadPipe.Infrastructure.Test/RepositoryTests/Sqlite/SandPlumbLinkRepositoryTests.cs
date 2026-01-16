using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Repository;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Test.RepositoryTests.Sqlite;

public class SandPlumbLinkRepositoryTests
{
    private readonly SandEntity _sandy = new() { Id = 0, CustardId = 0, Offerman = string.Empty };
    private readonly PlumbingEntity _plumb = new() { Id = 0, MetaData = string.Empty };
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
            new() {
                SandId = 1, SandEntity = _sandy , PlumbingId = 1, MatchingPhone = 12345, PlumbingEntity = _plumb },
            new() {
                SandId = 2, SandEntity = _sandy, PlumbingId = 2, MatchingPhone = 67890, PlumbingEntity = _plumb }
        };

        var result = await repo.UpsertRangeAsync(links);

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
        Assert.Contains("No link entities", result.Error);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldFail_WhenNullList()
    {
        var context = RepoTestHelpers.GetInMemoryContext();

        var repo = new SandPlumbingLinkRepository(context, logger);

        var result = await repo.UpsertRangeAsync([]);

        Assert.False(result.IsSuccess);
        Assert.Contains("No link entities", result.Error);
    }

    [Fact]
    public async Task AddAsync_ShouldAddLink()
    {
        var context = RepoTestHelpers.GetInMemoryContext();

        var repo = new SandPlumbingLinkRepository(context, logger);

        var link = new SandPlumbingLink { SandId = 1, SandEntity = _sandy, PlumbingId = 1, MatchingPhone = 12345, PlumbingEntity = _plumb };
        var result = await repo.UpsertRangeAsync([link]);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnLink_WhenExists()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        context.SandPlumbingLinks.Add(new SandPlumbingLink { SandId = 1, SandEntity = _sandy, PlumbingId = 1, MatchingPhone = 12345, PlumbingEntity = _plumb });
        await context.SaveChangesAsync();

        var repo = new SandPlumbingLinkRepository(context, logger);
        var result = await repo.FindAsync(l => l.Id == 1);

        Assert.True(result.IsSuccess);
        Assert.Equal(12345, result.Value[0].MatchingPhone);
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
    public async Task UpdateValuesAsync_ShouldUpdateLink()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var link = new SandPlumbingLink { SandId = 1, SandEntity = _sandy, PlumbingId = 1, MatchingPhone = 12345, PlumbingEntity = _plumb };
        context.SandPlumbingLinks.Add(link);
        await context.SaveChangesAsync();

        var repo = new SandPlumbingLinkRepository(context, logger);
        var updatedLink = new SandPlumbingLink { SandId = 1, SandEntity = _sandy, PlumbingId = 1, MatchingPhone = 67890, PlumbingEntity = _plumb };

        var result = await repo.UpsertRangeAsync([updatedLink]);
        var reloaded = await repo.FindAsync(l => l.Id == 1);

        Assert.True(result.IsSuccess);
        Assert.True(reloaded.IsSuccess);
        Assert.Equal(67890, reloaded.Value[0].MatchingPhone);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldFail_WhenEntityDoesNotExist()
    {
        var logger = LoggerFactory
            .Create(builder => builder.AddConsole())
            .CreateLogger<SandPlumbingLinkRepository>();

        var repo = new SandPlumbingLinkRepository(RepoTestHelpers.GetInMemoryContext(), logger);
        var updatedLink = new SandPlumbingLink { SandId = 99, SandEntity = _sandy, PlumbingId = 99, MatchingPhone = 11111, PlumbingEntity = _plumb };

        var result = await repo.UpsertRangeAsync([updatedLink]);

        Assert.False(result.IsSuccess);
        Assert.Contains("does not exist", result.Error);
    }
}
