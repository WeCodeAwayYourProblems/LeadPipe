using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using LeadPipe.Infrastructure.Sqlite.Repository;
using LeadPipe.Infrastructure.Test.RepositoryTests.MySql;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace LeadPipe.Infrastructure.Test.RepositoryTests.Sqlite;

[Trait("Category", "Integration")]
public class CaliperRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly PlumbingContext _context;
    private readonly CaliperRepository _repo;

    public CaliperRepositoryTests()
    {
        _context = SqliteTestContextFactory.Create(out _connection);

        _repo = new CaliperRepository(
            _context,
            NullLogger<CaliperRepository>.Instance
        );
    }

    [Fact]
    public async Task UpsertRangeAsync_Inserts_New_Entities()
    {
        var entities = new List<CaliperEntity>
    {
        new()
        {
            Id = 1,
            PhoneNumber = new(2345678901),
            Date = DateTime.UtcNow,
            UnixDate = 111,
            Duration = 30,
            Billable = true,
            Note = string.Empty,
            Source = string.Empty,
            Location = string.Empty
        }
    };

        var result = await _repo.UpsertRangeAsync(entities);

        ResultAssertions.ShouldBeSuccess(result);

        var saved = await _context.CaliperEntities.ToListAsync();
        Assert.Single(saved);
    }

    [Fact]
    public async Task UpsertRangeAsync_Updates_Existing_Entities()
    {
        var date = DateTime.UtcNow;

        await _repo.UpsertRangeAsync(
            new()
            {
            new CaliperEntity
            {
                Id = 1,
                PhoneNumber = new(2345678910),
                Date = date,
                UnixDate = 111,
                Duration = 30,
                Billable = false,
                Note = string.Empty,
                Source = string.Empty,
                Location = string.Empty
            }
            });

        await _repo.UpsertRangeAsync(
            [new CaliperEntity
            {
                Id = 1,
                PhoneNumber = new(2345678910),
                Date = date,
                UnixDate = 222,
                Duration = 60,
                Billable = true,
                Note = string.Empty,
                Source = string.Empty,
                Location = string.Empty
            }]);

        var entity = await _context.CaliperEntities.SingleAsync();

        Assert.Equal(222, entity.UnixDate);
        Assert.Equal(60, entity.Duration);
        Assert.True(entity.Billable);
    }

    [Fact]
    public async Task FindWithDetailsAsync_Includes_Links()
    {
        var caliper = new CaliperEntity
        {
            Id = 1,
            PhoneNumber = new(2345678910),
            Date = DateTime.UtcNow,
            UnixDate = 1,
            Note = string.Empty,
            Source = string.Empty,
            Location = string.Empty
        };

        _context.CaliperEntities.Add(caliper);
        await _context.SaveChangesAsync();

        var result = await _repo.FindWithDetailsAsync(c => c.Id == 1);

        ResultAssertions.ShouldBeSuccess(result);
        Assert.Single(result.Value!);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

}
