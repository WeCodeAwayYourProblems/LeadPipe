using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using LeadPipe.Infrastructure.Sqlite.Repository;
using LeadPipe.Infrastructure.Test.RepositoryTests.MySql;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace LeadPipe.Infrastructure.Test.RepositoryTests.Sqlite;

[Trait("Category", "Integration")]
public class CornCaliperLinkRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly PlumbingContext _context;
    private readonly CornCaliperLinkRepository _repo;

    public CornCaliperLinkRepositoryTests()
    {
        _context = SqliteTestContextFactory.Create(out _connection);

        _repo = new CornCaliperLinkRepository(
            _context,
            NullLogger<CornCaliperLinkRepository>.Instance
        );
    }

    [Fact]
    public async Task UpsertRangeAsync_With_Empty_List_Returns_Success()
    {
        var result = await _repo.UpsertRangeAsync([]);

        ResultAssertions.ShouldBeSuccess(result);
        Assert.Empty(result.Value!);
    }
    [Fact]
    public async Task UpsertRangeAsync_Inserts_New_Links()
    {
        // Arrange: required FK rows
        _context.CornEntities.Add(new CornEntity
        {
            Id = 1,
            PhoneNumber = new(2345678910),
            Source = string.Empty,
            MetaData = "{}",
            Payload = "{}",
            Date = DateTime.UtcNow,
            UnixDate = 1
        });

        _context.CaliperEntities.Add(new CaliperEntity
        {
            Id = 10,
            PhoneNumber = new(2345678910),
            Date = DateTime.UtcNow,
            UnixDate = 1,
            Note = string.Empty,
            Source = string.Empty,
            Location = string.Empty
        });

        await _context.SaveChangesAsync();

        var link = new CornCaliperLink
        {
            CornId = 1,
            CaliperId = 10,
            MatchingPhone = 123,
            UnixMatchDate = 7568920
        };

        // Act
        var result = await _repo.UpsertRangeAsync([link]);

        // Assert
        ResultAssertions.ShouldBeSuccess(result);

        var saved = await _context.CornCaliperLinks.ToListAsync();
        Assert.Single(saved);
        Assert.Equal(123, saved[0].MatchingPhone);
    }

    [Fact]
    public async Task UpsertRangeAsync_Deduplicates_By_Composite_Key()
    {
        SeedCornAndCaliper(1, 10);

        List<CornCaliperLink> links =
            [
                new CornCaliperLink { CornId = 1, CaliperId = 10, MatchingPhone = 111, UnixMatchDate = 7568921 },
                new CornCaliperLink { CornId = 1, CaliperId = 10, MatchingPhone = 222, UnixMatchDate = 7568922 }
            ];

        var result = await _repo.UpsertRangeAsync(links);

        ResultAssertions.ShouldBeSuccess(result);

        var saved = await _context.CornCaliperLinks.SingleAsync();
        Assert.Equal(222, saved.MatchingPhone);
    }

    [Fact]
    public async Task UpsertRangeAsync_Updates_Existing_Link()
    {
        SeedCornAndCaliper(1, 10);

        _context.CornCaliperLinks.Add(new CornCaliperLink
        {
            CornId = 1,
            CaliperId = 10,
            MatchingPhone = 111,
            UnixMatchDate = 7568921
        });
        await _context.SaveChangesAsync();

        var update = new CornCaliperLink
        {
            CornId = 1,
            CaliperId = 10,
            MatchingPhone = 999,
            UnixMatchDate = 7568921
        };

        var result = await _repo.UpsertRangeAsync([update]);

        ResultAssertions.ShouldBeSuccess(result);

        var saved = await _context.CornCaliperLinks.SingleAsync();
        Assert.Equal(999, saved.MatchingPhone);
    }

    private void SeedCornAndCaliper(long cornId, long caliperId)
    {
        _context.CornEntities.Add(new CornEntity
        {
            Id = cornId,
            PhoneNumber = new(2345678910),
            Source = string.Empty,
            MetaData = "{}",
            Payload = "{}",
            Date = DateTime.UtcNow,
            UnixDate = 1
        });

        _context.CaliperEntities.Add(new CaliperEntity
        {
            Id = caliperId,
            PhoneNumber = new(2345678910),
            Date = DateTime.UtcNow,
            UnixDate = 1,
            Note = string.Empty,
            Location = string.Empty,
            Source = string.Empty
        });

        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
