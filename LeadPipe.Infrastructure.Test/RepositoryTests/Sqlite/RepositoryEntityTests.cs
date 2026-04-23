using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Infrastructure.Sqlite.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LeadPipe.Infrastructure.Test.RepositoryTests.Sqlite;

public class RepositoryEntityTests
{
    [Fact]
    public async Task Caliper_UpsertEntityRangeAsync_InsertsAndUpdatesCorrectly()
    {
        // Arrange
        var context = SqliteTestContextFactory.Create(out _);
        var logger = Substitute.For<ILogger<CaliperRepository>>();
        var repo = new CaliperRepository(context, logger);

        // Seed
        var number = new PhoneNumber(5555555555);
        var entity = new CaliperEntity
        {
            Id = 1,
            PhoneNumber = number,
            Date = DateTime.UtcNow,
            UnixDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Note = "Note1",
            Label = "label",
            Source = "Source1",
            Location = "Location1",
            Duration = 123,
            Billable = true
        };

        var entities = new List<CaliperEntity> { entity };

        // Act (Insert)
        var result = await repo.UpsertRangeAsync(entities);
        Assert.True(result.IsSuccess);

        var inserted = await context.Set<CaliperEntity>().AsNoTracking().SingleAsync();
        Assert.Equal(entity.Id, inserted.Id);
        Assert.Equal(entity.PhoneNumber.Number, inserted.PhoneNumber.Number);
        Assert.Equal(entity.Note, inserted.Note);

        // Act (Update)
        inserted.Note = "UpdatedNote";
        inserted.Duration = 999;
        await repo.UpsertRangeAsync(new List<CaliperEntity> { inserted });

        var updated = await context.Set<CaliperEntity>().AsNoTracking().SingleAsync();
        Assert.Equal("UpdatedNote", updated.Note);
        Assert.Equal(999, updated.Duration);
    }

    [Fact]
    public async Task Corn_UpsertEntityRangeAsync_InsertsAndUpdatesCorrectly()
    {
        // Arrange
        var context = SqliteTestContextFactory.Create(out _);
        var logger = Substitute.For<ILogger<CornRepository>>();
        var repo = new CornRepository(context, logger);

        // Seed a CornEntity
        var number = new PhoneNumber(5555555555);
        var now = DateTime.UtcNow;
        var entity = new CornEntity
        {
            Id = 1,
            PhoneNumber = number,
            Date = now,
            UnixDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Payload = "InitialPayload",
            MetaData = "InitialMeta",
            Source = "Source1"
        };

        var entities = new List<CornEntity> { entity };

        // Act (Insert)
        var insertResult = await repo.UpsertRangeAsync(entities);
        Assert.True(insertResult.IsSuccess);

        var inserted = await context.Set<CornEntity>().AsNoTracking().SingleAsync();
        Assert.Equal(entity.Id, inserted.Id);
        Assert.Equal(entity.PhoneNumber.Number, inserted.PhoneNumber.Number);
        Assert.Equal("InitialPayload", inserted.Payload);
        Assert.Equal("InitialMeta", inserted.MetaData);
        Assert.Equal("Source1", inserted.Source);

        // Act (Update) - change some properties
        var updatedEntity = new CornEntity
        {
            Id = 1,
            PhoneNumber = new PhoneNumber(9999999999),
            Date = now.AddDays(1),
            UnixDate = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds(),
            Payload = "UpdatedPayload",
            MetaData = "UpdatedMeta",
            Source = "SourceUpdated"
        };

        var updateResult = await repo.UpsertRangeAsync(new List<CornEntity> { updatedEntity });
        Assert.True(updateResult.IsSuccess);

        var updated = await context.Set<CornEntity>().AsNoTracking().SingleAsync();
        Assert.Equal(1, updated.Id);
        Assert.Equal(9999999999, updated.PhoneNumber.Number);
        Assert.Equal("UpdatedPayload", updated.Payload);
        Assert.Equal("UpdatedMeta", updated.MetaData);
        Assert.Equal("SourceUpdated", updated.Source);
    }

    [Fact]
    public async Task Custard_UpsertEntityRangeAsync_InsertsAndUpdatesCorrectly()
    {
        // Arrange
        var context = SqliteTestContextFactory.Create(out _);
        var logger = Substitute.For<ILogger<CustardRepository>>();
        var repo = new CustardRepository(context, logger);

        var now = DateTime.UtcNow;
        var actualNumber = 5555555555;
        var number = new PhoneNumber(actualNumber);

        // Seed CustardEntity
        var entity = new CustardEntity
        {
            Id = 1,
            Active = true,
            PhoneNumber = number,
            PhoneNumber2 = null,
            Date = now,
            UnixDate = new DateTimeOffset(now).ToUnixTimeSeconds(),
            UnixCancelDate = new DateTimeOffset(now.AddDays(10)).ToUnixTimeSeconds()
        };

        var entities = new List<CustardEntity> { entity };

        // Act (Insert)
        var insertResult = await repo.UpsertRangeAsync(entities);
        Assert.True(insertResult.IsSuccess);

        var inserted = await context.Set<CustardEntity>().AsNoTracking().SingleAsync();
        Assert.Equal(entity.Id, inserted.Id);
        Assert.Equal(actualNumber, inserted.PhoneNumber.Number);
        Assert.Equal(now.ToString(), inserted.Date.ToString());
        Assert.Equal(now.AddDays(10).ToString(), inserted.CancelDate.ToString());
        Assert.True(inserted.Active);

        // Act (Update) - modify some properties
        var updatedEntity = new CustardEntity
        {
            Id = 1,
            Active = false,
            PhoneNumber = new PhoneNumber(9999999999),
            PhoneNumber2 = new PhoneNumber(8888888888),
            Date = now.AddDays(1),
            UnixDate = new DateTimeOffset(now.AddDays(1)).ToUnixTimeSeconds(),
            UnixCancelDate = new DateTimeOffset(now.AddDays(20)).ToUnixTimeSeconds()
        };

        var updateResult = await repo.UpsertRangeAsync(new List<CustardEntity> { updatedEntity });
        Assert.True(updateResult.IsSuccess);

        var updated = await context.Set<CustardEntity>().AsNoTracking().SingleAsync();
        Assert.Equal(1, updated.Id);
        Assert.Equal(9999999999, updated.PhoneNumber.Number);
        Assert.Equal(8888888888, updated.PhoneNumber2!.Number);
        Assert.Equal(now.AddDays(1).ToString(), updated.Date.ToString());
        Assert.Equal(now.AddDays(20).ToString(), updated.CancelDate.ToString());
        Assert.False(updated.Active);
    }

    [Fact]
    public async Task Plumbing_UpsertEntityRangeAsync_InsertsAndUpdatesCorrectly()
    {
        // Arrange
        var context = SqliteTestContextFactory.Create(out _);
        var logger = NSubstitute.Substitute.For<ILogger<PlumbingRepository>>();
        var translator = Substitute.For<IPlumbingMetaDataCanonicalPersistenceFormat<PlumbingEntity, string>>();
        var repo = new PlumbingRepository(context, translator, logger);

        var now = DateTime.UtcNow;
        var number = new PhoneNumber(5555555555);

        // Seed PlumbingEntity
        var entity = new PlumbingEntity
        {
            Id = 1,
            PhoneNumber = number,
            Date = now,
            UnixDate = new DateTimeOffset(now).ToUnixTimeSeconds(),
            MetaData = "Meta1",
            Source = Source.Test, // assuming Source is a valid instance
            Contents = "Initial contents",
            Branch = "Main"
        };

        var entities = new List<PlumbingEntity> { entity };

        // Act (Insert)
        var insertResult = await repo.UpsertRangeAsync(entities);
        Assert.True(insertResult.IsSuccess);

        var inserted = await context.Set<PlumbingEntity>().AsNoTracking().SingleAsync();
        Assert.Equal(entity.Id, inserted.Id);
        Assert.Equal(5555555555, inserted.PhoneNumber.Number);
        Assert.Equal("Meta1", inserted.MetaData);
        Assert.Equal("Initial contents", inserted.Contents);
        Assert.Equal("Main", inserted.Branch);
        Assert.Equal(Source.Test, inserted.Source);

        // Act (Update) - modify some properties
        var updatedEntity = new PlumbingEntity
        {
            Id = 1,
            PhoneNumber = number,
            Date = now,
            UnixDate = new DateTimeOffset(now).ToUnixTimeSeconds(),
            MetaData = "Meta1",
            Source = Source.Test,
            Contents = "Updated contents",
            Branch = "Branch2"
        };

        var updateResult = await repo.UpsertRangeAsync(new List<PlumbingEntity> { updatedEntity });
        Assert.True(updateResult.IsSuccess);

        var updated = await context.Set<PlumbingEntity>().AsNoTracking().SingleAsync();
        Assert.Equal(1, updated.Id);
        Assert.Equal(5555555555, updated.PhoneNumber.Number);
        Assert.Equal("Meta1", updated.MetaData);
        Assert.Equal(Source.Test, updated.Source);
        Assert.Equal("Updated contents", updated.Contents);
        Assert.Equal("Branch2", updated.Branch);

    }

    [Fact]
    public async Task Sand_UpsertEntityRangeAsync_RespectsDeduplicationAndCustardValidation()
    {
        // Arrange
        var context = SqliteTestContextFactory.Create(out _);
        var logger = NSubstitute.Substitute.For<ILogger<SandRepository>>();
        var repo = new SandRepository(context, logger);

        var now = DateTime.UtcNow;
        var custard = new CustardEntity
        {
            Id = 1,
            PhoneNumber = new PhoneNumber(5555555555),
            Date = now,
            UnixDate = new DateTimeOffset(now).ToUnixTimeSeconds(),
            UnixCancelDate = new DateTimeOffset(now).ToUnixTimeSeconds(),
            Active = true,
        };

        await context.CustardEntities.AddAsync(custard);
        await context.SaveChangesAsync();

        // Valid SandEntities
        var sand1 = new SandEntity
        {
            Id = 1,
            CustardId = 1, // exists
            Date = now,
            UnixDate = new DateTimeOffset(now).ToUnixTimeSeconds(),
            UnixCancelDate = new DateTimeOffset(now).ToUnixTimeSeconds(),
            Active = true,
            Complete = false,
            Value = 123.45m,
            Offerman = "Alice",
            Seller = 1,
            Seller2 = 2,
            Seller3 = 3
        };

        // Duplicate Id - should pick the last one
        var sand2 = new SandEntity
        {
            Id = 1,
            CustardId = 1,
            Date = now.AddHours(1),
            UnixDate = new DateTimeOffset(now.AddHours(1)).ToUnixTimeSeconds(),
            UnixCancelDate = new DateTimeOffset(now).ToUnixTimeSeconds(),
            Active = false,
            Complete = true,
            Value = 999.99m,
            Offerman = "Bob",
            Seller = 10,
            Seller2 = 20,
            Seller3 = 30
        };

        // Invalid CustardId - should be skipped
        var sandInvalid = new SandEntity
        {
            Id = 2,
            CustardId = 999, // does not exist
            Date = now,
            UnixDate = new DateTimeOffset(now).ToUnixTimeSeconds(),
            UnixCancelDate = new DateTimeOffset(now).ToUnixTimeSeconds(),
            Active = true,
            Complete = false,
            Value = 111.11m,
            Offerman = "Charlie",
            Seller = 0,
            Seller2 = 0,
            Seller3 = 0
        };

        var entities = new List<SandEntity> { sand1, sand2, sandInvalid };

        // Act
        var result = await repo.UpsertRangeAsync(entities);
        Assert.True(result.IsSuccess);

        var inserted = await context.SandEntities.AsNoTracking().SingleAsync();

        // Deduplication: last entity with same Id wins
        Assert.Equal(1, inserted.Id);
        Assert.Equal("Bob", inserted.Offerman);
        Assert.Equal(999.99m, inserted.Value);
        Assert.True(inserted.Complete);
        Assert.False(inserted.Active); // updated value
        Assert.Equal(10, inserted.Seller);
        Assert.Equal(20, inserted.Seller2);
        Assert.Equal(30, inserted.Seller3);

        // Confirm invalid entity was skipped
        var allSand = await context.SandEntities.AsNoTracking().ToListAsync();
        Assert.Single(allSand);
        Assert.DoesNotContain(allSand, s => s.Id == 2);

        // Act (Update) - modify some properties
        inserted.Value = 555.55m;
        inserted.Offerman = "Updated";
        inserted.Complete = false;

        var updateResult = await repo.UpsertRangeAsync(new List<SandEntity> { inserted });
        Assert.True(updateResult.IsSuccess);

        var updated = await context.SandEntities.AsNoTracking().SingleAsync();
        Assert.Equal(555.55m, updated.Value);
        Assert.Equal("Updated", updated.Offerman);
        Assert.False(updated.Complete);
    }

}
