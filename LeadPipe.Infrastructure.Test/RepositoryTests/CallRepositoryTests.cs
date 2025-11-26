using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Repository;
using CSharpFunctionalExtensions;

namespace LeadPipe.Infrastructure.Test.RepositoryTests;

public class CallRepositoryTests
{

    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleEntities()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var repo = new CallRepository(context);

        var entities = new List<CallEntity>
        {
            new() { Id = 1, PhoneNumber = 12345 },
            new() { Id = 2, PhoneNumber = 67890 }
        };

        var result = await repo.AddRangeAsync(entities);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, context.CallEntities.Count());
    }

    [Fact]
    public async Task AddRangeAsync_ShouldFail_WhenEmptyList()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var repo = new CallRepository(context);

        var result = await repo.AddRangeAsync([]);

        Assert.False(result.IsSuccess);
        Assert.Contains("No plumbing entities", result.Error);
    }

    [Fact]
    public async Task AddAsync_ShouldAddCallEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var repo = new CallRepository(context);

        var plumbing = new CallEntity { Id = 1, PhoneNumber = 12345};
        Result result = await repo.AddAsync(plumbing);

        Assert.True(result.IsSuccess);
    }
    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        context.CallEntities.Add(new CallEntity { Id = 1, PhoneNumber = 12345 });
        await context.SaveChangesAsync();

        var repo = new CallRepository(context);
        var result = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(12345, result.Value.PhoneNumber);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var plumbing = new CallEntity { Id = 1, PhoneNumber = 12345 };
        context.CallEntities.Add(plumbing);
        await context.SaveChangesAsync();

        var repo = new CallRepository(context);
        var result = await repo.DeleteAsync(1);
        var reloaded = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.True(reloaded.IsFailure);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldFail_WhenNotFound()
    {
        var repo = new CallRepository(RepoTestHelpers.GetInMemoryContext());
        var result = await repo.GetByIdAsync(99);

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldUpdateEntity()
    {
        var context = RepoTestHelpers.GetInMemoryContext();
        var plumbing = new CallEntity { Id = 1, PhoneNumber = 12345 };
        context.CallEntities.Add(plumbing);
        await context.SaveChangesAsync();

        var repo = new CallRepository(context);
        var updatedCall = new CallEntity { Id = 1, PhoneNumber = 99999 };

        var result = await repo.UpdateAsync(updatedCall);
        var reloaded = await repo.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.True(reloaded.IsSuccess);
        Assert.Equal(99999, reloaded.Value.PhoneNumber);
    }

    [Fact]
    public async Task UpdateValuesAsync_ShouldFail_WhenEntityDoesNotExist()
    {
        var repo = new CallRepository(RepoTestHelpers.GetInMemoryContext());
        var updatedCall = new CallEntity { Id = 99, PhoneNumber = 11111 };

        var result = await repo.UpdateAsync(updatedCall);

        Assert.False(result.IsSuccess);
        Assert.Contains("does not exist", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSucceed_WhenEntityDoesNotExist()
    {
        var repo = new CallRepository(RepoTestHelpers.GetInMemoryContext());
        var result = await repo.DeleteAsync(99);

        Assert.True(result.IsSuccess);
    }
}
