using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Repository;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Test.RepositoryTests.Sqlite;

public class SandRepositoryTests
{

    private readonly ILogger<SandRepository> logger = LoggerFactory
            .Create(builder => builder.AddConsole())
            .CreateLogger<SandRepository>();
    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleSubsEntities()
    {
        var context = RepoTestHelpers.GetInMemoryContext();

        var repo = new SandRepository(context, logger);

        var entities = new List<SandEntity>
        {
            new() { Id = 1, CustardId = 0, Offerman = string.Empty },
            new() { Id = 2, CustardId = 0, Offerman = string.Empty }
        };

        var result = await repo.UpsertRangeAsync(entities);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, context.SandEntities.Count());
    }

    [Fact]
    public async Task AddRangeAsync_ShouldFail_WhenEmptyList()
    {
        var context = RepoTestHelpers.GetInMemoryContext();

        var repo = new SandRepository(context, logger);

        var result = await repo.UpsertRangeAsync([]);

        Assert.True(result.IsFailure);
        Assert.Contains("No entities", result.Error);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldFail_WhenNullList()
    {
        var context = RepoTestHelpers.GetInMemoryContext();

        var repo = new SandRepository(context, logger);

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        var result = await repo.UpsertRangeAsync(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        Assert.True(result.IsFailure);
        Assert.Contains("No entities", result.Error);
    }

    [Fact]
    public async Task AddAsync_ShouldAddSubsEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();

        var repo = new SandRepository(context, logger);

        var subs = new SandEntity { Id = 1, CustardId = 0, Offerman = string.Empty };
        var result = await repo.UpsertRangeAsync([subs]);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        context.SandEntities.Add(new SandEntity { Id = 1, CustardId = 0, Offerman = string.Empty });
        await context.SaveChangesAsync();

        var repo = new SandRepository(context, logger);
        var result = await repo.FindAsync(l => l.Id == 1);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldFail_WhenNotFound()
    {
        var repo = new SandRepository(RepoTestHelpers.GetInMemoryContext(), logger);
        var result = await repo.FindAsync(l => l.Id == 99);

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldUpdateEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var subs = new SandEntity { Id = 1, CustardId = 0, Offerman = string.Empty };
        context.SandEntities.Add(subs);
        await context.SaveChangesAsync();

        var repo = new SandRepository(context, logger);
        var updatedSubs = new SandEntity { Id = 1, CustardId = 0, Offerman = string.Empty };

        var result = await repo.UpsertRangeAsync([updatedSubs]);
        var reloaded = await repo.FindAsync(l => l.Id == 1);

        Assert.True(result.IsSuccess);
        Assert.True(reloaded.IsSuccess);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldFail_WhenEntityDoesNotExist()
    {
        var repo = new SandRepository(RepoTestHelpers.GetInMemoryContext(), logger);
        var updatedSubs = new SandEntity { Id = 99, CustardId = 0, Offerman = string.Empty };

        var result = await repo.UpsertRangeAsync([updatedSubs]);

        Assert.False(result.IsSuccess);
        Assert.Contains("does not exist", result.Error);
    }
}
