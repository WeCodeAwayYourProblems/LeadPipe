using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using LeadPipe.Infrastructure.Sqlite.Repository;
using LeadPipe.Infrastructure.Test.RepositoryTests.MySql;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace LeadPipe.Infrastructure.Test.RepositoryTests.Sqlite;

[Trait("Category", "Integration")]
public class CornPlumbingLinkRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly PlumbingContext _context;
    private readonly CornPlumbingLinkRepository _repo;

    public CornPlumbingLinkRepositoryTests()
    {
        _context = SqliteTestContextFactory.Create(out _connection);

        _repo = new CornPlumbingLinkRepository(
            _context,
            NullLogger<CornPlumbingLinkRepository>.Instance
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
        SeedCornAndPlumbing(1, 10);

        var link = new CornPlumbingLink
        {
            CornId = 1,
            PlumbingId = 10,
            MatchingPhone = 555,
            UnixMatchDate = 7568921
        };

        var result = await _repo.UpsertRangeAsync([link]);

        ResultAssertions.ShouldBeSuccess(result);

        var saved = await _context.CornPlumbingLinks.ToListAsync();
        Assert.Single(saved);
        Assert.Equal(555, saved[0].MatchingPhone);
    }

    [Fact]
    public async Task UpsertRangeAsync_Deduplicates_By_Composite_Key()
    {
        SeedCornAndPlumbing(1, 10);

        List<CornPlumbingLink> links =
            [
                new CornPlumbingLink { CornId = 1, PlumbingId = 10, MatchingPhone = 111, UnixMatchDate = 7568921 },
                new CornPlumbingLink { CornId = 1, PlumbingId = 10, MatchingPhone = 222, UnixMatchDate = 7568920 }
            ];

        var result = await _repo.UpsertRangeAsync(links);

        ResultAssertions.ShouldBeSuccess(result);

        var saved = await _context.CornPlumbingLinks.SingleAsync();
        Assert.Equal(222, saved.MatchingPhone);
    }

    [Fact]
    public async Task UpsertRangeAsync_Updates_Existing_Link()
    {
        SeedCornAndPlumbing(1, 10);

        _context.CornPlumbingLinks.Add(new CornPlumbingLink
        {
            CornId = 1,
            PlumbingId = 10,
            MatchingPhone = 123,
            UnixMatchDate = 7568921
        });
        await _context.SaveChangesAsync();

        var update = new CornPlumbingLink
        {
            CornId = 1,
            PlumbingId = 10,
            MatchingPhone = 999,
            UnixMatchDate = 7568921
        };

        var result = await _repo.UpsertRangeAsync([update]);

        ResultAssertions.ShouldBeSuccess(result);

        var saved = await _context.CornPlumbingLinks.SingleAsync();
        Assert.Equal(999, saved.MatchingPhone);
    }

    [Fact]
    public async Task GetAllWithDetailsAsync_Loads_Navigation_Properties()
    {
        SeedCornAndPlumbing(1, 10);

        _context.CornPlumbingLinks.Add(new CornPlumbingLink
        {
            CornId = 1,
            PlumbingId = 10,
            MatchingPhone = 555,
            UnixMatchDate = 7568921
        });

        // Add Corn
        DateTime now = DateTime.UtcNow;
        DateTimeOffset nowOffset = new(now, TimeSpan.Zero);
        CornEntity corn = new()
        {
            Id = 1,
            PhoneNumber = new(2345678910),
            Date = now,
            UnixDate = nowOffset.ToUnixTimeSeconds(),
            Payload = string.Empty,
            MetaData = string.Empty,
            Source = string.Empty
        };
        _context.CornEntities.Add(corn);

        // Add Plumbing
        PlumbingEntity plumb = new()
        {
            Id = 10,
            PhoneNumber = new(2345678910),
            MetaData = string.Empty
        };
        _context.PlumbingEntities.Add(plumb);

        await _context.SaveChangesAsync();

        var result = await _repo.GetAllWithDetailsAsync();

        ResultAssertions.ShouldBeSuccess(result);

        var link = Assert.Single(result.Value!);
        Assert.NotNull(link.CornEntity);
        Assert.NotNull(link.PlumbingEntity);
    }

    private void SeedCornAndPlumbing(long cornId, long plumbingId)
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

        _context.PlumbingEntities.Add(new PlumbingEntity
        {
            Id = plumbingId,
            PhoneNumber = new(2345678910),
            Source = Domain.ValueObjects.Source.Test,
            MetaData = string.Empty
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
