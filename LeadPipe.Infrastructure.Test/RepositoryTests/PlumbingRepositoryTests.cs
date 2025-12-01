using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Repository;
using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Sqlite.Repository;
using LeadPipe.Infrastructure.Entity.Sqlite;

namespace LeadPipe.Infrastructure.Test.RepositoryTests;

public class PlumbingRepositoryTests
{

    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleEntities()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var repo = new PlumbingRepository(context);

        var entities = new List<PlumbingEntity>
        {
            new() { Id = 1, PhoneNumber = 12345 },
            new() { Id = 2, PhoneNumber = 67890 }
        };

        var result = await repo.AddRangeAsync(entities);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, context.PlumbingEntities.Count());
    }

    [Fact]
    public async Task AddRangeAsync_ShouldFail_WhenEmptyList()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var repo = new PlumbingRepository(context);

        var result = await repo.AddRangeAsync([]);

        Assert.False(result.IsSuccess);
        Assert.Contains("No plumbing entities", result.Error);
    }

    [Fact]
    public async Task AddAsync_ShouldAddPlumbingEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var repo = new PlumbingRepository(context);

        var plumbing = new PlumbingEntity { Id = 1, PhoneNumber = 12345, Source = Source.Test };
        Result result = await repo.AddAsync(plumbing);

        Assert.True(result.IsSuccess);
    }
    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        context.PlumbingEntities.Add(new PlumbingEntity { Id = 1, PhoneNumber = 12345 });
        await context.SaveChangesAsync();

        var repo = new PlumbingRepository(context);
        var result = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(12345, result.Value.PhoneNumber);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var plumbing = new PlumbingEntity { Id = 1, PhoneNumber = 12345 };
        context.PlumbingEntities.Add(plumbing);
        await context.SaveChangesAsync();

        var repo = new PlumbingRepository(context);
        var result = await repo.DeleteAsync(1);
        var reloaded = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.True(reloaded.IsFailure);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldFail_WhenNotFound()
    {
        var repo = new PlumbingRepository(RepoTestHelpers.GetInMemoryContext());
        var result = await repo.GetByIdAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldUpdateEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var plumbing = new PlumbingEntity { Id = 1, PhoneNumber = 12345 };
        context.PlumbingEntities.Add(plumbing);
        await context.SaveChangesAsync();

        var repo = new PlumbingRepository(context);
        var updatedPlumbing = new PlumbingEntity { Id = 1, PhoneNumber = 99999 };

        var result = await repo.UpdateAsync(updatedPlumbing);
        var reloaded = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.True(reloaded.IsSuccess);
        Assert.Equal(99999, reloaded.Value.PhoneNumber);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldFail_WhenEntityDoesNotExist()
    {
        var repo = new PlumbingRepository(RepoTestHelpers.GetInMemoryContext());
        var updatedPlumbing = new PlumbingEntity { Id = 99, PhoneNumber = 11111 };

        var result = await repo.UpdateAsync(updatedPlumbing);

        Assert.False(result.IsSuccess);
        Assert.Contains("does not exist", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSucceed_WhenEntityDoesNotExist()
    {
        var repo = new PlumbingRepository(RepoTestHelpers.GetInMemoryContext());
        var result = await repo.DeleteAsync(99);

        Assert.True(result.IsSuccess);
    }
}
