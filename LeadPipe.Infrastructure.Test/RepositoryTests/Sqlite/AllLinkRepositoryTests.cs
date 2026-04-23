using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;
using LeadPipe.Infrastructure.Sqlite.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LeadPipe.Infrastructure.Test.RepositoryTests.Sqlite;

public class AllLinkRepositoryTests
{
    #region Setup
    private static readonly PlumbingContext context = SqliteTestContextFactory.Create(out _);
    private static DateTime Now => DateTime.UtcNow;
    private static long UnixNow => new DateTimeOffset(Now).ToUnixTimeSeconds();

    private static PhoneNumber TestNumber => new(5555555555);

    private static CornEntity CreateCorn(long id = 1) =>
        new()
        {
            Id = id,
            PhoneNumber = TestNumber,
            Date = Now,
            UnixDate = UnixNow,
            Payload = "",
            MetaData = "",
            Source = ""
        };

    private static PlumbingEntity CreatePlumbing(long id = 2) =>
        new()
        {
            Id = id,
            PhoneNumber = TestNumber,
            Date = Now,
            UnixDate = UnixNow,
            MetaData = "",
            Source = Source.Test
        };

    private static CaliperEntity CreateCaliper(long id = 3) =>
        new()
        {
            Id = id,
            PhoneNumber = TestNumber,
            Note = "",
            Source = "",
            Label = "",
            Location = ""
        };

    private static CustardEntity CreateCustard(long id = 4) =>
        new()
        {
            Id = id,
            Active = true,
            PhoneNumber = TestNumber,
            Date = Now,
            UnixDate = UnixNow,
            UnixCancelDate = UnixNow
        };

    private static SandEntity CreateSand(long id = 5, long custardId = 4) =>
        new()
        {
            Id = id,
            CustardId = custardId,
            Date = Now,
            UnixDate = UnixNow,
            UnixCancelDate = UnixNow,
            Active = true,
            Complete = false,
            Value = 1,
            Offerman = "Test",
            Seller = 1,
            Seller2 = 2,
            Seller3 = 3
        };

    private static async Task AssertEarliestWinsAndUpdates<TLink>(
        DbContext context,
        Func<List<TLink>, Task> upsert,
        Func<TLink> duplicateHigh,
        Func<TLink> duplicateLow,
        Func<TLink> ignored,
        Func<TLink> update,
        Func<TLink, long> getMatchDate,
        Func<TLink, long> getPhone)
        where TLink : class
    {
        await upsert(new() { duplicateHigh(), duplicateLow(), ignored() });

        var inserted = await context.Set<TLink>().AsNoTracking().SingleAsync();
        Assert.Equal(100, getMatchDate(inserted));
        Assert.Equal(555, getPhone(inserted));

        await upsert(new() { update() });

        var updated = await context.Set<TLink>().AsNoTracking().SingleAsync();
        Assert.Equal(10, getMatchDate(updated));
        Assert.Equal(999, getPhone(updated));
    }

    #endregion

    [Fact]
    public async Task CornPlumbingLink_EarliestWins()
    {
        var repo = new CornPlumbingLinkRepository(context,
            Substitute.For<ILogger<CornPlumbingLinkRepository>>());

        context.AddRange(CreateCorn(1), CreatePlumbing(2));
        await context.SaveChangesAsync();

        await AssertEarliestWinsAndUpdates(
            context,
            links => repo.UpsertLinkRangeAsync(links, CancellationToken.None),
            () => new CornPlumbingLink { CornId = 1, PlumbingId = 2, MatchingPhone = 555, UnixMatchDate = 200 },
            () => new CornPlumbingLink { CornId = 1, PlumbingId = 2, MatchingPhone = 555, UnixMatchDate = 100 },
            () => new CornPlumbingLink { CornId = 1, PlumbingId = 2, MatchingPhone = 0, UnixMatchDate = 50 },
            () => new CornPlumbingLink { CornId = 1, PlumbingId = 2, MatchingPhone = 999, UnixMatchDate = 10 },
            x => x.UnixMatchDate,
            x => x.MatchingPhone);
    }

    [Fact]
    public async Task CustardCaliperLink_EarliestWins()
    {
        var repo = new CustardCaliperLinkRepository(context,
            Substitute.For<ILogger<CustardCaliperLinkRepository>>());

        context.AddRange(CreateCustard(1), CreateCaliper(2));
        await context.SaveChangesAsync();

        await AssertEarliestWinsAndUpdates(
            context,
            links => repo.UpsertLinkRangeAsync(links, CancellationToken.None),
            () => new CustardCaliperLink { CustardId = 1, CaliperId = 2, MatchingPhone = 555, UnixMatchDate = 200 },
            () => new CustardCaliperLink { CustardId = 1, CaliperId = 2, MatchingPhone = 555, UnixMatchDate = 100 },
            () => new CustardCaliperLink { CustardId = 1, CaliperId = 2, MatchingPhone = 0, UnixMatchDate = 50 },
            () => new CustardCaliperLink { CustardId = 1, CaliperId = 2, MatchingPhone = 999, UnixMatchDate = 10 },
            x => x.UnixMatchDate,
            x => x.MatchingPhone);
    }

    [Fact]
    public async Task CustardCornLink_EarliestWins()
    {
        var repo = new CustardCornLinkRepository(context,
            Substitute.For<ILogger<CustardCornLinkRepository>>());

        context.AddRange(CreateCustard(1), CreateCorn(2));
        await context.SaveChangesAsync();

        await AssertEarliestWinsAndUpdates(
            context,
            links => repo.UpsertLinkRangeAsync(links, CancellationToken.None),
            () => new CustardCornLink { CustardId = 1, CornId = 2, MatchingPhone = 555, UnixMatchDate = 200 },
            () => new CustardCornLink { CustardId = 1, CornId = 2, MatchingPhone = 555, UnixMatchDate = 100 },
            () => new CustardCornLink { CustardId = 1, CornId = 2, MatchingPhone = 0, UnixMatchDate = 50 },
            () => new CustardCornLink { CustardId = 1, CornId = 2, MatchingPhone = 999, UnixMatchDate = 10 },
            x => x.UnixMatchDate,
            x => x.MatchingPhone);
    }

    [Fact]
    public async Task CustardPlumbingLink_EarliestWins()
    {
        var repo = new CustardPlumbingLinkRepository(context,
            Substitute.For<ILogger<CustardPlumbingLinkRepository>>());

        context.AddRange(CreateCustard(1), CreatePlumbing(2));
        await context.SaveChangesAsync();

        await AssertEarliestWinsAndUpdates(
            context,
            links => repo.UpsertLinkRangeAsync(links, CancellationToken.None),
            () => new CustardPlumbingLink { CustardId = 1, PlumbingId = 2, MatchingPhone = 555, UnixMatchDate = 200 },
            () => new CustardPlumbingLink { CustardId = 1, PlumbingId = 2, MatchingPhone = 555, UnixMatchDate = 100 },
            () => new CustardPlumbingLink { CustardId = 1, PlumbingId = 2, MatchingPhone = 0, UnixMatchDate = 50 },
            () => new CustardPlumbingLink { CustardId = 1, PlumbingId = 2, MatchingPhone = 999, UnixMatchDate = 10 },
            x => x.UnixMatchDate,
            x => x.MatchingPhone);
    }

    [Fact]
    public async Task PlumbingCaliperLink_EarliestWins()
    {
        var repo = new PlumbingCaliperLinkRepository(context,
            Substitute.For<ILogger<PlumbingCaliperLinkRepository>>());

        context.AddRange(CreatePlumbing(1), CreateCaliper(2));
        await context.SaveChangesAsync();

        await AssertEarliestWinsAndUpdates(
            context,
            links => repo.UpsertLinkRangeAsync(links, CancellationToken.None),
            () => new PlumbingCaliperLink { PlumbingId = 1, CaliperId = 2, MatchingPhone = 555, UnixMatchDate = 200 },
            () => new PlumbingCaliperLink { PlumbingId = 1, CaliperId = 2, MatchingPhone = 555, UnixMatchDate = 100 },
            () => new PlumbingCaliperLink { PlumbingId = 1, CaliperId = 2, MatchingPhone = 0, UnixMatchDate = 50 },
            () => new PlumbingCaliperLink { PlumbingId = 1, CaliperId = 2, MatchingPhone = 999, UnixMatchDate = 10 },
            x => x.UnixMatchDate,
            x => x.MatchingPhone);
    }

    [Fact]
    public async Task SandCaliperLink_EarliestWins()
    {
        var repo = new SandCaliperLinkRepository(context,
            Substitute.For<ILogger<SandCaliperLinkRepository>>());

        var custard = CreateCustard(1);
        var sand = CreateSand(2, 1);
        var caliper = CreateCaliper(3);

        context.AddRange(custard, sand, caliper);
        await context.SaveChangesAsync();

        await AssertEarliestWinsAndUpdates(
            context,
            links => repo.UpsertLinkRangeAsync(links, CancellationToken.None),
            () => new SandCaliperLink { SandId = 2, CaliperId = 3, MatchingPhone = 555, UnixMatchDate = 200 },
            () => new SandCaliperLink { SandId = 2, CaliperId = 3, MatchingPhone = 555, UnixMatchDate = 100 },
            () => new SandCaliperLink { SandId = 2, CaliperId = 3, MatchingPhone = 0, UnixMatchDate = 50 },
            () => new SandCaliperLink { SandId = 2, CaliperId = 3, MatchingPhone = 999, UnixMatchDate = 10 },
            x => x.UnixMatchDate,
            x => x.MatchingPhone);
    }

    [Fact]
    public async Task SandCornLink_EarliestWins()
    {
        var repo = new SandCornLinkRepository(context,
            Substitute.For<ILogger<SandCornLinkRepository>>());

        var custard = CreateCustard(1);
        var sand = CreateSand(2, 1);
        var corn = CreateCorn(3);

        context.AddRange(custard, sand, corn);
        await context.SaveChangesAsync();

        await AssertEarliestWinsAndUpdates(
            context,
            links => repo.UpsertLinkRangeAsync(links, CancellationToken.None),
            () => new SandCornLink { SandId = 2, CornId = 3, MatchingPhone = 555, UnixMatchDate = 200 },
            () => new SandCornLink { SandId = 2, CornId = 3, MatchingPhone = 555, UnixMatchDate = 100 },
            () => new SandCornLink { SandId = 2, CornId = 3, MatchingPhone = 0, UnixMatchDate = 50 },
            () => new SandCornLink { SandId = 2, CornId = 3, MatchingPhone = 999, UnixMatchDate = 10 },
            x => x.UnixMatchDate,
            x => x.MatchingPhone);
    }

    [Fact]
    public async Task SandPlumbingLink_EarliestWins()
    {
        var repo = new SandPlumbingLinkRepository(context,
            Substitute.For<ILogger<SandPlumbingLinkRepository>>());

        var custard = CreateCustard(1);
        var sand = CreateSand(2, 1);
        var plumbing = CreatePlumbing(3);

        context.AddRange(custard, sand, plumbing);
        await context.SaveChangesAsync();

        await AssertEarliestWinsAndUpdates(
            context,
            links => repo.UpsertLinkRangeAsync(links, CancellationToken.None),
            () => new SandPlumbingLink { SandId = 2, PlumbingId = 3, MatchingPhone = 555, UnixMatchDate = 200 },
            () => new SandPlumbingLink { SandId = 2, PlumbingId = 3, MatchingPhone = 555, UnixMatchDate = 100 },
            () => new SandPlumbingLink { SandId = 2, PlumbingId = 3, MatchingPhone = 0, UnixMatchDate = 50 },
            () => new SandPlumbingLink { SandId = 2, PlumbingId = 3, MatchingPhone = 999, UnixMatchDate = 10 },
            x => x.UnixMatchDate,
            x => x.MatchingPhone);
    }

}
