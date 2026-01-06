using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Repository;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Test.RepositoryTests;

public class SubsRepositoryTests
{

    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleSubsEntities()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var logger = LoggerFactory
            .Create(builder => builder.AddConsole())
            .CreateLogger<SubsRepository>();
        var repo = new SubsRepository(context, logger);

        var entities = new List<SubsEntity>
        {
            new() { Id = 1, Number = 12345, Number2 = 67890 },
            new() { Id = 2, Number = 11111, Number2 = 22222 }
        };

        var result = await repo.AddRangeAsync(entities);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, context.SubsEntities.Count());
    }

    [Fact]
    public async Task AddRangeAsync_ShouldFail_WhenEmptyList()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var logger = LoggerFactory
            .Create(builder => builder.AddConsole())
            .CreateLogger<SubsRepository>();
        var repo = new SubsRepository(context, logger);

        var result = await repo.AddRangeAsync([]);

        Assert.True(result.IsFailure);
        Assert.Contains("No entities", result.Error);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldFail_WhenNullList()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var logger = LoggerFactory
            .Create(builder => builder.AddConsole())
            .CreateLogger<SubsRepository>();
        var repo = new SubsRepository(context, logger);

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        var result = await repo.AddRangeAsync(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        Assert.True(result.IsFailure);
        Assert.Contains("No entities", result.Error);
    }

    [Fact]
    public async Task AddAsync_ShouldAddSubsEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var logger = LoggerFactory
            .Create(builder => builder.AddConsole())
            .CreateLogger<SubsRepository>();
        var repo = new SubsRepository(context, logger);

        var subs = new SubsEntity { Id = 1, Number = 12345, Number2 = 67890 };
        var result = await repo.AddAsync(subs);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        context.SubsEntities.Add(new SubsEntity { Id = 1, Number = 12345 });
        await context.SaveChangesAsync();

        var logger = LoggerFactory
            .Create(builder => builder.AddConsole())
            .CreateLogger<SubsRepository>();
        var repo = new SubsRepository(context, logger);
        var result = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(12345, result.Value.Number);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldFail_WhenNotFound()
    {
        var logger = LoggerFactory
            .Create(builder => builder.AddConsole())
            .CreateLogger<SubsRepository>();
        var repo = new SubsRepository(RepoTestHelpers.GetInMemoryContext(), logger);
        var result = await repo.GetByIdAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldUpdateEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var subs = new SubsEntity { Id = 1, Number = 12345 };
        context.SubsEntities.Add(subs);
        await context.SaveChangesAsync();

        var logger = LoggerFactory
            .Create(builder => builder.AddConsole())
            .CreateLogger<SubsRepository>();
        var repo = new SubsRepository(context, logger);
        var updatedSubs = new SubsEntity { Id = 1, Number = 99999, Number2 = 67890 };

        var result = await repo.UpdateAsync(updatedSubs);
        var reloaded = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.True(reloaded.IsSuccess);
        Assert.Equal(99999, reloaded.Value.Number);
        Assert.Equal(67890, reloaded.Value.Number2);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var subs = new SubsEntity { Id = 1, Number = 12345 };
        context.SubsEntities.Add(subs);
        await context.SaveChangesAsync();

        var logger = LoggerFactory
            .Create(builder => builder.AddConsole())
            .CreateLogger<SubsRepository>();
        var repo = new SubsRepository(context, logger);
        var result = await repo.DeleteAsync(1);
        var reloaded = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.True(reloaded.IsFailure);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldFail_WhenEntityDoesNotExist()
    {
        var logger = LoggerFactory
            .Create(builder => builder.AddConsole())
            .CreateLogger<SubsRepository>();
        var repo = new SubsRepository(RepoTestHelpers.GetInMemoryContext(), logger);
        var updatedSubs = new SubsEntity { Id = 99, Number = 11111 };

        var result = await repo.UpdateAsync(updatedSubs);

        Assert.False(result.IsSuccess);
        Assert.Contains("does not exist", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSucceed_WhenEntityDoesNotExist()
    {
        var logger = LoggerFactory
            .Create(builder => builder.AddConsole())
            .CreateLogger<SubsRepository>();
        var repo = new SubsRepository(RepoTestHelpers.GetInMemoryContext(), logger);
        var result = await repo.DeleteAsync(99);

        Assert.True(result.IsSuccess); // Deleting non-existent entity should still succeed
    }
}
