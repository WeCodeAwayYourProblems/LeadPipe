using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Repository;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Test.RepositoryTests.Sqlite;

public class CaliperRepositoryTests
{
    private readonly ILogger<CaliperRepository> logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<CaliperRepository>();
    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleEntities()
    {
        var context = RepoTestHelpers.GetInMemoryContext();

        var repo = new CaliperRepository(context, logger);

        var entities = new List<CaliperEntity>
        {
            new() { Id = 1, PhoneNumber = 12345, Note = string.Empty, Location = string.Empty, Source = string.Empty },
            new() { Id = 2, PhoneNumber = 67890, Note = string.Empty, Location = string.Empty, Source = string.Empty }
        };

        var result = await repo.UpsertRangeAsync(entities);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, context.CaliperEntities.Count());
    }

    [Fact]
    public async Task AddRangeAsync_ShouldFail_WhenEmptyList()
    {
        var context = RepoTestHelpers.GetInMemoryContext();

        var repo = new CaliperRepository(context, logger);

        var result = await repo.UpsertRangeAsync([]);

        Assert.False(result.IsSuccess);
        Assert.Contains("No plumbing entities", result.Error);
    }

    [Fact]
    public async Task AddAsync_ShouldAddCaliperEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();

        var repo = new CaliperRepository(context, logger);

        var plumbing = new CaliperEntity { Id = 1, PhoneNumber = 12345, Note = string.Empty, Location = string.Empty, Source = string.Empty };
        Result result = await repo.UpsertRangeAsync([plumbing]);

        Assert.True(result.IsSuccess);
    }
    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        context.CaliperEntities.Add(new CaliperEntity { Id = 1, PhoneNumber = 12345, Note = string.Empty, Location = string.Empty, Source = string.Empty });
        await context.SaveChangesAsync();


        var repo = new CaliperRepository(context, logger);
        Result<List<CaliperEntity>> result = await repo.FindAsync(c => c.Id == 1);

        Assert.True(result.IsSuccess);
        Assert.Equal(12345, result.Value[0].PhoneNumber);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldFail_WhenNotFound()
    {

        var repo = new CaliperRepository(RepoTestHelpers.GetInMemoryContext(), logger);
        var result = await repo.FindAsync(l => l.Id == 99);

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldUpdateEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var plumbing = new CaliperEntity { Id = 1, PhoneNumber = 12345, Note = string.Empty, Location = string.Empty, Source = string.Empty };
        context.CaliperEntities.Add(plumbing);
        await context.SaveChangesAsync();


        var repo = new CaliperRepository(context, logger);
        var updatedCaliper = new CaliperEntity { Id = 1, PhoneNumber = 99999, Note = string.Empty, Location = string.Empty, Source = string.Empty };

        var result = await repo.UpsertRangeAsync([updatedCaliper]);
        var reloaded = await repo.FindAsync(l => l.Id == 1);

        Assert.True(result.IsSuccess);
        Assert.True(reloaded.IsSuccess);
        Assert.Equal(99999, reloaded.Value[0].PhoneNumber);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldFail_WhenEntityDoesNotExist()
    {

        var repo = new CaliperRepository(RepoTestHelpers.GetInMemoryContext(), logger);
        var updatedCaliper = new CaliperEntity { Id = 99, PhoneNumber = 11111, Note = string.Empty, Location = string.Empty, Source = string.Empty };

        var result = await repo.UpsertRangeAsync([updatedCaliper]);

        Assert.False(result.IsSuccess);
        Assert.Contains("does not exist", result.Error);
    }
}
