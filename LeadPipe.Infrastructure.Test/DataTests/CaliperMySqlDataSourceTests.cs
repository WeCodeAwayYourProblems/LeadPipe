using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Data.DataSource;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository;
using NSubstitute;
using System.Linq.Expressions;

namespace LeadPipe.Infrastructure.Test.DataTests;

internal class TestClock : IClock
{
    public DateTimeOffset UtcNow { get; set; }
}
public class CaliperMySqlDataSourceTests
{
    private readonly ICaliperMySqlRepository _repo = Substitute.For<ICaliperMySqlRepository>();
    private readonly ISyncStateRepository _sync = Substitute.For<ISyncStateRepository>();
    private readonly IClock _clock = new TestClock { UtcNow = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero) };

    private readonly SyncedCaliperMySqlDataSource _sut;

    public CaliperMySqlDataSourceTests()
    {
        _sut = new SyncedCaliperMySqlDataSource(_repo, _sync, _clock);
    }

    [Fact]
    public async Task LoadAsync_ShouldLoadEntities_AndPersistSyncState()
    {
        // Arrange
        var entities = new List<CaliperMySqlEntity>
        {
            new() { called_at_utc = DateTime.UtcNow.AddDays(-1) },
            new() { called_at_utc = DateTime.UtcNow }
        };

        _repo.FindAsync(Arg.Any<Expression<Func<CaliperMySqlEntity, bool>>>(), true)
            .Returns(Result.Success(entities));

        _sync.UpsertRangeAsync(Arg.Any<List<SyncStateEntity>>())
            .Returns(Result.Success(new List<SyncStateEntity>()));

        // Act
        var result = await _sut.LoadAsync(true);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);

        await _sync.Received(1)
            .UpsertRangeAsync(Arg.Any<List<SyncStateEntity>>());
    }

    [Fact]
    public async Task RefreshAsync_ShouldUseLatestSyncDate()
    {
        // Arrange
        var lastSync = DateTimeOffset.UtcNow.AddDays(-10);

        _sync.GetByKeyAsync(null, SyncKey.Caliper)
            .Returns(Result.Success(new SyncStateEntity
            {
                BusinessId = BusinessId.From(SyncKey.Caliper.Value),
                UnixLastSyncUtc = lastSync.ToUnixTimeSeconds()
            }));

        var refreshed = new List<CaliperMySqlEntity>
        {
            new() { called_at_utc = DateTime.UtcNow }
        };

        _repo.FindAsync(Arg.Any<Expression<Func<CaliperMySqlEntity, bool>>>(), false)
            .Returns(Result.Success(refreshed));

        _sync.UpsertRangeAsync(Arg.Any<List<SyncStateEntity>>())
            .Returns(Result.Success(new List<SyncStateEntity>()));

        // Act
        var result = await _sut.RefreshAsync(false);

        // Assert
        Assert.True(result.IsSuccess);

        await _repo.Received(1)
            .FindAsync(Arg.Any<Expression<Func<CaliperMySqlEntity, bool>>>(), false);

        await _sync.Received(1)
            .UpsertRangeAsync(Arg.Any<List<SyncStateEntity>>());
    }

}
