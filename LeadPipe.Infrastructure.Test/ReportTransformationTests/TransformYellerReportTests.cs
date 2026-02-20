using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Data.Transform;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Infrastructure.Settings;
using LeadPipe.Translation.Translate.EntityToReport;
using NSubstitute;
using System.Linq.Expressions;

namespace LeadPipe.Infrastructure.Test.ReportTransformationTests;

public class TransformYellerReportTests
{
    private static TransformYellerReport CreateSut(
    IQueryable<CustardEntity> custards,
    IQueryable<PlumbingEntity> plumbing,
    IQueryable<CornEntity> corn,
    IQueryable<CaliperEntity> caliper)
    {
        var factory = Substitute.For<IRepositoryFactory>();
        var custardRepo = Substitute.For<IRepository<CustardEntity>>();
        var plumbRepo = Substitute.For<IRepository<PlumbingEntity>>();
        var cornRepo = Substitute.For<IRepository<CornEntity>>();
        var caliperRepo = Substitute.For<IRepository<CaliperEntity>>();
        var translator = Substitute.For<IEntityToReport<AttributionResult, ReportYeller>>();
        var settings = Substitute.For<IYellerSettings>();

        // Provide default YellerCaliperSource values
        settings.YellerCaliperSource1.Returns("source1");
        settings.YellerCaliperSource2.Returns("source2");

        // Factory returns the mocks
        factory.GetRepository<CustardEntity>().Returns(custardRepo);
        factory.GetRepository<PlumbingEntity>().Returns(plumbRepo);
        factory.GetRepository<CornEntity>().Returns(cornRepo);
        factory.GetRepository<CaliperEntity>().Returns(caliperRepo);

        // Helper
        static IQueryable<T> SafeQuery<T>(IQueryable<T>? q) => q ?? Enumerable.Empty<T>().AsQueryable();

        // Custard FindWithDetailsAsync
        custardRepo
            .FindWithDetailsAsync(Arg.Any<Expression<Func<CustardEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var predicate = ci.Arg<Expression<Func<CustardEntity, bool>>>();
                var data = SafeQuery(custards).Where(predicate).ToList();
                return Task.FromResult(Result.Success(data));
            });

        // Other entities FindAsync
        plumbRepo.FindAsync(Arg.Any<Expression<Func<PlumbingEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(ci => Task.FromResult(Result.Success(SafeQuery(plumbing).Where(ci.Arg<Expression<Func<PlumbingEntity, bool>>>()).ToList())));
        cornRepo.FindAsync(Arg.Any<Expression<Func<CornEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(ci => Task.FromResult(Result.Success(SafeQuery(corn).Where(ci.Arg<Expression<Func<CornEntity, bool>>>()).ToList())));
        caliperRepo.FindAsync(Arg.Any<Expression<Func<CaliperEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(ci => Task.FromResult(Result.Success(SafeQuery(caliper).Where(ci.Arg<Expression<Func<CaliperEntity, bool>>>()).ToList())));

        // Translator
        translator.Translate(Arg.Any<AttributionResult>()).Returns(ci =>
        {
            var result = ci.Arg<AttributionResult>();
            string num1 = YellerReportHelper.HashSha256(result.Custard.PhoneNumber.Number.ToString());
            string num2 = result.Custard.PhoneNumber2 is null
                ? PhoneNumber.Default.ToString()
                : YellerReportHelper.HashSha256(result.Custard.PhoneNumber2.Number.ToString());

            return new ReportYeller
            {
                event_id = result.Custard.Id.ToString(),
                event_time = result.FirstTouchUnixDate,
                event_name = "purchase",
                user_data = new UserData { ph = new[] { num1, num2 } },
                custom_data = new CustomData
                {
                    currency = YellerReportHelper.Currency,
                    value = result.Value
                }
            };
        });

        return new TransformYellerReport(factory, translator, settings);
    }

    [Fact]
    public async Task Plumbing_FirstTouch_With_Valid_Sand()
    {
        AttributionTestBuilder b = new AttributionTestBuilder();
        CustardEntity c = b.Custard(1, 2000);
        PlumbingEntity p = b.Plumbing(10, 1000);
        b.PlumbingLink(c, p, 1000);
        b.Sand(c, 100, 3000, true);

        var sut = CreateSut(new[] { c }.AsQueryable());

        var result = await sut.TransformAsync([]);

        Assert.Single(result.Value);
    }

    [Fact]
    public async Task Only_First_Chronological_Sand()
    {
        var b = new AttributionTestBuilder();
        var c = b.Custard(1, 2000);
        var p = b.Plumbing(10, 1000);
        b.PlumbingLink(c, p, 1000);

        b.Sand(c, 1, 3000, true, 300);
        b.Sand(c, 2, 2500, true, 200);
        b.Sand(c, 3, 4000, true, 500);
 
        var sut = CreateSut(new[] { c }.AsQueryable());

        var result = await sut.TransformAsync([]);

        Assert.Single(result.Value);
    }

    [Fact]
    public async Task Sand_Not_Complete_No_Report()
    {
        var b = new AttributionTestBuilder();
        var c = b.Custard(1, 2000);
        var p = b.Plumbing(10, 1000);
        b.PlumbingLink(c, p, 1000);
        b.Sand(c, 100, 3000, false);

        var sut = CreateSut(new[] { c }.AsQueryable());

        var result = await sut.TransformAsync([]);

        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Sand_Before_Plumbing_Invalid()
    {
        var b = new AttributionTestBuilder();
        var c = b.Custard(1, 2000);
        var p = b.Plumbing(10, 1000);
        b.PlumbingLink(c, p, 1000);
        b.Sand(c, 100, 900, true);

        var sut = CreateSut(new[] { c }.AsQueryable());

        var result = await sut.TransformAsync([]);

        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Plumbing_After_Custard_Invalidates()
    {
        var b = new AttributionTestBuilder();
        var c = b.Custard(1, 2000);
        var p = b.Plumbing(10, 3000);
        b.PlumbingLink(c, p, 3000);
        b.Sand(c, 100, 4000, true);

        var sut = CreateSut(new[] { c }.AsQueryable());

        var result = await sut.TransformAsync([]);

        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task FirstTouch_Across_All_Entities()
    {
        var b = new AttributionTestBuilder();
        var c = b.Custard(1, 4000);

        b.CaliperLink(c, 1000);
        b.CornLink(c, 2000);
        var p = b.Plumbing(10, 3000);
        b.PlumbingLink(c, p, 3000);

        b.Sand(c, 100, 5000, true);

        var sut = CreateSut(new[] { c }.AsQueryable());

        var result = await sut.TransformAsync([]);

        Assert.Equal(1000, result.Value.Single().event_time);
    }

    [Fact]
    public async Task Deterministic_TieBreaker_By_Link_Id()
    {
        var b = new AttributionTestBuilder();
        var c = b.Custard(1, 2000);

        b.CornLink(c, 1000, id: 10);
        b.CaliperLink(c, 1000, id: 5); // lower id should win

        b.Sand(c, 100, 3000, true);

        var sut = CreateSut(new[] { c }.AsQueryable());

        var result = await sut.TransformAsync([]);

        Assert.Equal("1", result.Value.Single().event_id);
        Assert.Equal(1000, result.Value.Single().event_time);
    }

    [Fact]
    public async Task Multiple_Custards_Same_Phone_First_Wins()
    {
        var b = new AttributionTestBuilder();

        var c1 = b.Custard(1, 2000);
        var c2 = b.Custard(2, 2000);

        var p1 = b.Plumbing(10, 1000);
        var p2 = b.Plumbing(11, 1500);

        b.PlumbingLink(c1, p1, 1000);
        b.PlumbingLink(c2, p2, 1500);

        b.Sand(c1, 1, 3000, true);
        b.Sand(c2, 2, 3000, true);

        var sut = CreateSut(new[] { c1, c2 }.AsQueryable());

        var result = await sut.TransformAsync([]);

        Assert.Single(result.Value);
        Assert.Equal("1", result.Value.Single().event_id);
    }

    [Fact]
    public async Task Multiple_Phones_Independent()
    {
        var b1 = new AttributionTestBuilder();
        var b2 = new AttributionTestBuilder();

        var c1 = b1.Custard(1, 2000);
        var c2 = b2.Custard(2, 2000);

        b1.PlumbingLink(c1, b1.Plumbing(10, 1000), 1000);
        b2.PlumbingLink(c2, b2.Plumbing(11, 1000), 1000);

        b1.Sand(c1, 1, 3000, true);
        b2.Sand(c2, 2, 3000, true);

        var sut = CreateSut(new[] { c1, c2 }.AsQueryable());

        var result = await sut.TransformAsync([]);

        Assert.Equal(2, result.Value.Count);
    }

    [Fact]
    public async Task No_Sand_No_Report()
    {
        var b = new AttributionTestBuilder();
        var c = b.Custard(1, 2000);
        b.PlumbingLink(c, b.Plumbing(10, 1000), 1000);

        var sut = CreateSut(new[] { c }.AsQueryable());

        var result = await sut.TransformAsync([]);

        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Sand_Before_Custard_But_After_Plumbing_Allowed()
    {
        var b = new AttributionTestBuilder();
        var c = b.Custard(1, 2000);
        b.PlumbingLink(c, b.Plumbing(10, 1000), 1000);
        b.Sand(c, 1, 1500, true);

        var sut = CreateSut(new[] { c }.AsQueryable());

        var result = await sut.TransformAsync([]);

        Assert.Single(result.Value);
    }

    [Fact]
    public async Task Value_Comes_From_First_Sand_Only()
    {
        var b = new AttributionTestBuilder();
        var c = b.Custard(1, 2000);
        b.PlumbingLink(c, b.Plumbing(10, 1000), 1000);

        b.Sand(c, 1, 2000, true, 100);
        b.Sand(c, 2, 3000, true, 500);

        var sut = CreateSut(new[] { c }.AsQueryable());

        var result = await sut.TransformAsync([]);

        Assert.Single(result.Value);

        var report = result.Value.Single();
        Assert.Equal(100, report.custom_data.value);
    }
}

internal sealed class AttributionTestBuilder
{
    public PhoneNumber Phone { get; } = new PhoneNumber("5551112222");

    public CustardEntity Custard(long id, long unixDate)
    {
        return new CustardEntity
        {
            Id = id,
            Active = true,
            PhoneNumber = Phone,
            UnixDate = unixDate,
            SandEntities = [],
            CustardPlumbingLinks = [],
            CustardCornLinks = [],
            CustardCaliperLinks = []
        };
    }

    public PlumbingEntity Plumbing(long id, long unixDate)
    {
        return new PlumbingEntity
        {
            Id = id,
            PhoneNumber = Phone,
            UnixDate = unixDate,
            MetaData = "x",
            Source = Source.Test
        };
    }

    public SandEntity Sand(CustardEntity custard, long id, long unixDate, bool complete = true, decimal value = 0)
    {
        var sand = new SandEntity
        {
            Id = id,
            CustardId = custard.Id,
            CustardEntity = custard,
            UnixDate = unixDate,
            Complete = complete,
            Active = true,
            Offerman = "bob",
            Value = value
        };

        custard.SandEntities.Add(sand);
        return sand;
    }

    public CustardPlumbingLink PlumbingLink(CustardEntity c, PlumbingEntity p, long matchDate, long id = 1)
    {
        var link = new CustardPlumbingLink
        {
            Id = id,
            CustardId = c.Id,
            Custard = c,
            PlumbingId = p.Id,
            Plumbing = p,
            MatchingPhone = Phone.Number,
            UnixMatchDate = matchDate
        };

        c.CustardPlumbingLinks.Add(link);
        return link;
    }

    public CustardCornLink CornLink(CustardEntity c, long matchDate, long id = 1)
    {
        var corn = new CornEntity
        {
            Id = id,
            PhoneNumber = Phone,
            UnixDate = matchDate,
            Date = DateTime.UtcNow,
            Payload = "x",
            MetaData = "x",
            Source = "x"
        };

        var link = new CustardCornLink
        {
            Id = id,
            CustardId = c.Id,
            Custard = c,
            CornId = corn.Id,
            Corn = corn,
            MatchingPhone = Phone.Number,
            UnixMatchDate = matchDate
        };

        c.CustardCornLinks.Add(link);
        return link;
    }

    public CustardCaliperLink CaliperLink(CustardEntity c, long matchDate, long id = 1)
    {
        var caliper = new CaliperEntity
        {
            Id = id,
            PhoneNumber = Phone,
            UnixDate = matchDate,
            Note = "x",
            Source = "x",
            Location = "x"
        };

        var link = new CustardCaliperLink
        {
            Id = id,
            CustardId = c.Id,
            Custard = c,
            CaliperId = caliper.Id,
            Caliper = caliper,
            MatchingPhone = Phone.Number,
            UnixMatchDate = matchDate
        };

        c.CustardCaliperLinks.Add(link);
        return link;
    }
}