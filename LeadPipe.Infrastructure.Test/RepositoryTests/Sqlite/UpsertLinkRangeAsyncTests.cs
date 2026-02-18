using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using LeadPipe.Infrastructure.Sqlite.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Test.RepositoryTests.Sqlite;

public class UpsertLinkRangeAsyncTests
{
    private readonly static PlumbingContext _context= SqliteTestContextFactory.Create(out _);

    [Fact]
    public async Task UpsertLinkRangeAsync_InsertsAndUpdatesCorrectly_WithEarliestMatchWinning()
    {
        // Arrange
        var context = _context;

        await context.Database.EnsureCreatedAsync();

        // Seed parents (required for FK EXISTS checks)
        PhoneNumber sharedNumber = new(5555555555);
        DateTimeOffset date = DateTimeOffset.UtcNow;
        var corn = new CornEntity { Id = 1, PhoneNumber = sharedNumber, Date = date.UtcDateTime, UnixDate = date.ToUnixTimeSeconds(), MetaData = "", Payload = "", Source = "" };
        var caliper = new CaliperEntity { Id = 2, PhoneNumber = sharedNumber, Note = "", Source = "", Location = "" };

        context.AddRange(corn, caliper);
        await context.SaveChangesAsync();

        var logger = NSubstitute.Substitute.For<ILogger<CornCaliperLinkRepository>>();

        var repo = new CornCaliperLinkRepository(context, logger);

        var links = new List<CornCaliperLink>
        {
            // First insert candidate
            new CornCaliperLink
            {
                CornId = 1,
                CaliperId = 2,
                MatchingPhone = 555,
                UnixMatchDate = 200
            },

            // Duplicate business key, earlier date (should win)
            new CornCaliperLink
            {
                CornId = 1,
                CaliperId = 2,
                MatchingPhone = 555,
                UnixMatchDate = 100
            },

            // Should be ignored (phone == 0)
            new CornCaliperLink
            {
                CornId = 1,
                CaliperId = 2,
                MatchingPhone = 0,
                UnixMatchDate = 50
            }
        };

        // Act (INSERT path)
        var result = await repo.UpsertLinkRangeAsync(links, CancellationToken.None);

        // Assert insert
        Assert.True(result.IsSuccess);

        var inserted = await context.Set<CornCaliperLink>().SingleAsync();

        Assert.Equal(1, inserted.CornId);
        Assert.Equal(2, inserted.CaliperId);

        // Earliest match date should win
        Assert.Equal(100, inserted.UnixMatchDate);
        Assert.Equal(555, inserted.MatchingPhone);

        // Act again (UPDATE path)
        var updateBatch = new List<CornCaliperLink>
        {
            new CornCaliperLink
            {
                CornId = 1,
                CaliperId = 2,
                MatchingPhone = 999,
                UnixMatchDate = 10   // earlier -> should overwrite
            }
        };

        var updateResult = await repo.UpsertLinkRangeAsync(updateBatch, CancellationToken.None);

        Assert.True(updateResult.IsSuccess);

        var updated = await context.Set<CornCaliperLink>()
            .AsNoTracking() // This must be here or the test will fail because of how upsert works on this puppy
            .SingleAsync();

        Assert.Equal(999, updated.MatchingPhone);
        Assert.Equal(10, updated.UnixMatchDate);
    }
}
