using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Data.Persistence;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using NSubstitute;

namespace LeadPipe.Infrastructure.Test.DataTests;

public class PersistenceTests
{
    #region PlumbingPersistence

    [Fact]
    public async Task PlumbingPersistence_SaveAsync_ReturnsSuccess()
    {
        var repo = Substitute.For<IPlumbingRepository>();
        var entity = new PlumbingEntity() { Id = 0, MetaData = string.Empty };
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

        var result = await persistence.SaveAsync(new List<PlumbingEntity> { new PlumbingEntity() { Id = 0, MetaData = string.Empty } });

        Assert.True(result.IsFailure);
        Assert.Equal("error", result.Error);
        await repo.Received(1).AddRangeAsync(Arg.Any<List<PlumbingEntity>>());
    }

    #endregion

    #region CaliperEntityPersistence

    [Fact]
    public async Task CaliperEntityPersistence_SaveAsync_ReturnsSuccess()
    {
        var repo = Substitute.For<ICaliperRepository>();
        var entity = new CaliperEntity() { Id = 0, Note = string.Empty, Location = string.Empty, Source = $"{Source.Test}" };
        repo.AddRangeAsync(Arg.Any<List<CaliperEntity>>())
            .Returns(Task.FromResult(Result.Success(new List<CaliperEntity> { entity })));

        var persistence = new CaliperEntityPersistence(repo);

        var result = await persistence.SaveAsync(new List<CaliperEntity> { entity });

        Assert.True(result.IsSuccess);
        await repo.Received(1).AddRangeAsync(Arg.Any<List<CaliperEntity>>());
    }

    [Fact]
    public async Task CaliperEntityPersistence_SaveAsync_ReturnsFailure()
    {
        var repo = Substitute.For<ICaliperRepository>();
        repo.AddRangeAsync(Arg.Any<List<CaliperEntity>>())
            .Returns(Task.FromResult(Result.Failure<List<CaliperEntity>>("error")));

        var persistence = new CaliperEntityPersistence(repo);

        var result = await persistence.SaveAsync(new List<CaliperEntity> { new CaliperEntity() { Id = 0, Note = string.Empty, Location = string.Empty, Source = $"{Source.Test}" } });

        Assert.True(result.IsFailure);
        Assert.Equal("error", result.Error);
        await repo.Received(1).AddRangeAsync(Arg.Any<List<CaliperEntity>>());
    }

    #endregion

    // Repeat the same pattern for each persistence class:
    // CaliperMySqlPersistence, CustardMySqlEntityPersistence, PlumbingCaliperLinkPersistence,
    // SandCaliperLinkPersistence, SandPersistence, SandPlumbingLinkPersistence,
    // SandMySqlEntityPersistence, SummaryMySqlEntityPersistence
}
