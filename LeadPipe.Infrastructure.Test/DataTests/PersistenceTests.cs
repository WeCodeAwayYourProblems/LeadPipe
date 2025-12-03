using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Data.Persistence;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Repository;
using NSubstitute;

namespace LeadPipe.Infrastructure.Test.DataTests;

public class PersistenceTests
{
    #region PlumbingPersistence

    [Fact]
    public async Task PlumbingPersistence_SaveAsync_ReturnsSuccess()
    {
        var repo = Substitute.For<IPlumbingRepository>();
        var entity = new PlumbingEntity();
        repo.AddRangeAsync(Arg.Any<List<PlumbingEntity>>())
            .Returns(Task.FromResult(Result.Success(new List<PlumbingEntity> { entity })));

        var persistence = new PlumbingPersistence(repo);

        var result = await persistence.SaveAsync(new List<PlumbingEntity> { entity });

        Assert.True(result.IsSuccess);
        await repo.Received(1).AddRangeAsync(Arg.Any<List<PlumbingEntity>>());
    }

    [Fact]
    public async Task PlumbingPersistence_SaveAsync_ReturnsFailure()
    {
        var repo = Substitute.For<IPlumbingRepository>();
        repo.AddRangeAsync(Arg.Any<List<PlumbingEntity>>())
            .Returns(Task.FromResult(Result.Failure<List<PlumbingEntity>>("error")));

        var persistence = new PlumbingPersistence(repo);

        var result = await persistence.SaveAsync(new List<PlumbingEntity> { new PlumbingEntity() });

        Assert.True(result.IsFailure);
        Assert.Equal("error", result.Error);
        await repo.Received(1).AddRangeAsync(Arg.Any<List<PlumbingEntity>>());
    }

    #endregion

    #region CallEntityPersistence

    [Fact]
    public async Task CallEntityPersistence_SaveAsync_ReturnsSuccess()
    {
        var repo = Substitute.For<ICallRepository>();
        var entity = new CallEntity() { Note = string.Empty, Source = $"{Source.Test}" };
        repo.AddRangeAsync(Arg.Any<List<CallEntity>>())
            .Returns(Task.FromResult(Result.Success(new List<CallEntity> { entity })));

        var persistence = new CallEntityPersistence(repo);

        var result = await persistence.SaveAsync(new List<CallEntity> { entity });

        Assert.True(result.IsSuccess);
        await repo.Received(1).AddRangeAsync(Arg.Any<List<CallEntity>>());
    }

    [Fact]
    public async Task CallEntityPersistence_SaveAsync_ReturnsFailure()
    {
        var repo = Substitute.For<ICallRepository>();
        repo.AddRangeAsync(Arg.Any<List<CallEntity>>())
            .Returns(Task.FromResult(Result.Failure<List<CallEntity>>("error")));

        var persistence = new CallEntityPersistence(repo);

        var result = await persistence.SaveAsync(new List<CallEntity> { new CallEntity() { Note = string.Empty, Source = $"{Source.Test}" } });

        Assert.True(result.IsFailure);
        Assert.Equal("error", result.Error);
        await repo.Received(1).AddRangeAsync(Arg.Any<List<CallEntity>>());
    }

    #endregion

    // Repeat the same pattern for each persistence class:
    // CallMySqlPersistence, CustardMySqlEntityPersistence, PlumbingCallLinkPersistence,
    // SubsCallLinkPersistence, SubsPersistence, SubsPlumbingLinkPersistence,
    // SubMySqlEntityPersistence, SummaryMySqlEntityPersistence
}
