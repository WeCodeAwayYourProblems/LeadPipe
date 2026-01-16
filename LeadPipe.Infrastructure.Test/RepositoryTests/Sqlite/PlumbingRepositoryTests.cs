using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Repository;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Test.RepositoryTests.Sqlite;

public class PlumbingRepositoryTests
{
    private readonly ILogger<PlumbingRepository> logger = LoggerFactory
        .Create(builder => builder.AddConsole())
        .CreateLogger<PlumbingRepository>();
    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleEntities()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var repo = new PlumbingRepository(context, logger);

        var entities = new List<PlumbingEntity>
        {
            new() { Id = 1, PhoneNumber = 12345, MetaData = string.Empty  },
            new() { Id = 2, PhoneNumber = 67890, MetaData = string.Empty  }
        };

        var result = await repo.UpsertRangeAsync(entities);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, context.PlumbingEntities.Count());
    }

    [Fact]
    public async Task AddRangeAsync_ShouldFail_WhenEmptyList()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var repo = new PlumbingRepository(context, logger);

        var result = await repo.UpsertRangeAsync([]);

        Assert.False(result.IsSuccess);
        Assert.Contains("No entities", result.Error);
    }

    [Fact]
    public async Task AddAsync_ShouldAddPlumbingEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var repo = new PlumbingRepository(context, logger);

        var plumbing = new PlumbingEntity { Id = 1, PhoneNumber = 12345, Source = Source.Test, MetaData = string.Empty };
        Result result = await repo.UpsertRangeAsync([plumbing]);

        Assert.True(result.IsSuccess);
    }
    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        context.PlumbingEntities.Add(new PlumbingEntity { Id = 1, PhoneNumber = 12345, MetaData = string.Empty });
        await context.SaveChangesAsync();

        var repo = new PlumbingRepository(context, logger);
        var result = await repo.FindAsync(l => l.Id == 1);

        Assert.True(result.IsSuccess);
        Assert.Equal(12345, result.Value[0].PhoneNumber);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldFail_WhenNotFound()
    {
        var repo = new PlumbingRepository(RepoTestHelpers.GetInMemoryContext(), logger);
        var result = await repo.FindAsync(l => l.Id == 99);

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldUpdateEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var plumbing = new PlumbingEntity { Id = 1, PhoneNumber = 12345, MetaData = string.Empty };
        context.PlumbingEntities.Add(plumbing);
        await context.SaveChangesAsync();

        var repo = new PlumbingRepository(context, logger);
        var updatedPlumbing = new PlumbingEntity { Id = 1, PhoneNumber = 99999, MetaData = string.Empty };

        var result = await repo.UpsertRangeAsync([updatedPlumbing]);
        var reloaded = await repo.FindAsync(l => l.Id == 1);

        Assert.True(result.IsSuccess);
        Assert.True(reloaded.IsSuccess);
        Assert.Equal(99999, reloaded.Value[0].PhoneNumber);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldFail_WhenEntityDoesNotExist()
    {
        var repo = new PlumbingRepository(RepoTestHelpers.GetInMemoryContext(), logger);
        var updatedPlumbing = new PlumbingEntity { Id = 99, PhoneNumber = 11111, MetaData = string.Empty };

        var result = await repo.UpsertRangeAsync([updatedPlumbing]);

        Assert.False(result.IsSuccess);
        Assert.Contains("does not exist", result.Error);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldNotAddDuplicates_WhenEntitiesExist()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var repo = new PlumbingRepository(context, logger);

        var entity = new PlumbingEntity { Id = 0, PhoneNumber = 12345, Source = Source.Test, MetaData = string.Empty };
        await repo.UpsertRangeAsync([entity]);

        // Attempt to add the same entity again
        var duplicates = new List<PlumbingEntity>
        {
            new() { Id=0, PhoneNumber = 12345, Source = Source.Test, MetaData = string.Empty  }
        };

        var result = await repo.UpsertRangeAsync(duplicates);

        Assert.True(result.IsSuccess);
        // No new rows should be added
        Assert.Equal(1, context.PlumbingEntities.Count());
        Assert.Empty(result.Value); // Added list should be empty because nothing new was inserted
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddOnlyNewEntities_WhenMixedWithExisting()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var repo = new PlumbingRepository(context, logger);

        // Existing entity in database
        var existing = new PlumbingEntity { Id = 0, PhoneNumber = 12345, Source = Source.Test, MetaData = string.Empty };
        await repo.UpsertRangeAsync([existing]);

        // New batch contains one existing + one new
        var batch = new List<PlumbingEntity>
        {
            new() { Id=0,PhoneNumber = 12345, Source = Source.Test, MetaData = string.Empty  }, // duplicate
            new() { Id=0,PhoneNumber = 67890, Source = Source.Test, MetaData = string.Empty  }  // new
        };

        var result = await repo.UpsertRangeAsync(batch);

        Assert.True(result.IsSuccess);
        // Total rows in DB = 2
        Assert.Equal(2, context.PlumbingEntities.Count());
        // Only the new entity is returned as added
        Assert.Single(result.Value);
        Assert.Equal(67890, result.Value[0].PhoneNumber);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldNotAddDuplicates_WhenEntitiesHaveSamePhoneNumberButDifferentSource()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var repo = new PlumbingRepository(context, logger);

        var entity = new PlumbingEntity { Id = 0, PhoneNumber = 12345, Source = Source.Test, MetaData = string.Empty };
        await repo.UpsertRangeAsync([entity]);

        // Same phone number but different source -> should be added
        var newEntity = new PlumbingEntity { Id = 0, PhoneNumber = 12345, Source = Source.Test2, MetaData = string.Empty };

        var result = await repo.UpsertRangeAsync([newEntity]);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, context.PlumbingEntities.Count());
        Assert.Single(result.Value);
        Assert.Equal(Source.Test2, result.Value[0].Source);
    }
}
