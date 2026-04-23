using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Data.Transform;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Infrastructure.Settings;
using NSubstitute;
using System.Linq.Expressions;

namespace LeadPipe.Infrastructure.Test.ReportTransformationTests;

public class TransformYellerReportTests
{
    #region CreateSut
    private static TransformYellerReport CreateSut(
    IQueryable<CustardEntity> custards,
    IQueryable<PlumbingEntity> plumbing,
    IQueryable<CornEntity> corn,
    IQueryable<CaliperEntity> caliper,
    List<AttributionResult> captured)
    {
        var repoFactory = Substitute.For<IRepositoryFactory>();
        var custardRepo = Substitute.For<IRepository<CustardEntity>>();
        var plumbRepo = Substitute.For<IRepository<PlumbingEntity>>();
        var cornRepo = Substitute.For<IRepository<CornEntity>>();
        var caliperRepo = Substitute.For<IRepository<CaliperEntity>>();

        var translateFactory = Substitute.For<IEntityToYellerReportFactory>();
        var attrToR = Substitute.For<IEntityToReport<AttributionResult, ReportYeller>>();
        var cornToR = Substitute.For<IEntityToReport<CornEntity, ReportYeller>>();
        var plumbToR = Substitute.For<IEntityToReport<PlumbingEntity, ReportYeller>>();
        var caliperToR = Substitute.For<IEntityToReport<CaliperEntity, ReportYeller>>();

        var settings = Substitute.For<IYellerSettings>();

        settings.YellerCaliperSource1.Returns("source1");
        settings.YellerCaliperSource2.Returns("source2");
        settings.YellerCornSource.Returns("source1");

        repoFactory.GetRepository<CustardEntity>().Returns(custardRepo);
        repoFactory.GetRepository<PlumbingEntity>().Returns(plumbRepo);
        repoFactory.GetRepository<CornEntity>().Returns(cornRepo);
        repoFactory.GetRepository<CaliperEntity>().Returns(caliperRepo);

        translateFactory.GetService<AttributionResult>().Returns(attrToR);
        translateFactory.GetService<CornEntity>().Returns(cornToR);
        translateFactory.GetService<PlumbingEntity>().Returns(plumbToR);
        translateFactory.GetService<CaliperEntity>().Returns(caliperToR);

        static IQueryable<T> SafeQuery<T>(IQueryable<T>? q)
            => q ?? Enumerable.Empty<T>().AsQueryable();

        custardRepo
            .FindWithDetailsAsync(Arg.Any<Expression<Func<CustardEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var predicate = ci.Arg<Expression<Func<CustardEntity, bool>>>();
                return Task.FromResult(Result.Success(
                    SafeQuery(custards).Where(predicate).ToList()));
            });

        plumbRepo
            .FindAsync(Arg.Any<Expression<Func<PlumbingEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var predicate = ci.Arg<Expression<Func<PlumbingEntity, bool>>>();
                return Task.FromResult(Result.Success(
                    SafeQuery(plumbing).Where(predicate).ToList()));
            });

        cornRepo
            .FindAsync(Arg.Any<Expression<Func<CornEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(SafeQuery(corn).ToList())));

        caliperRepo
            .FindAsync(Arg.Any<Expression<Func<CaliperEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(SafeQuery(caliper).ToList())));

        //attrToR
        //    .Translate(Arg.Any<AttributionResult>())
        //    .Returns(ci =>
        //    {
        //        var r = ci.Arg<AttributionResult>();
        //        captured.Add(r);

        //        return new ReportYeller
        //        {
        //            event_id = r.Custard.Id.ToString(),
        //            event_time = r.FirstTouchUnixDate,
        //            event_name = "purchase",
        //            action_source = "action",
        //            user_data = new UserData { ph = [r.Custard.PhoneNumber.Number.ToString(), r.Custard.PhoneNumber2?.Number.ToString() ?? PhoneNumber.Default.ToString()] },
        //            custom_data = new CustomData
        //            {
        //                currency = "USD",
        //                value = r.Value
        //            }
        //        };
        //    });

        // TODO: Mock entity translators.Translate method
        //cornToR
        //    .Translate(Arg.Any<CornEntity>())
        //    .Returns(ci =>
        //    {
        //        var r = ci.Arg<CornEntity>();
        //        captured.Add(ci);
        //    });
        //plumbToR; caliperToR;

        return new TransformYellerReport(repoFactory, translateFactory, settings);
    }
    #endregion

    [Fact]
    public async Task Plumbing_MultipleLinks_EarliestWins()
    {
        var b = new AttributionTestBuilder();

        var custard = b.Custard(1, 2000);

        var p1 = b.Plumbing(10, 1500);
        var p2 = b.Plumbing(20, 1000);
        var sand = b.Sand(custard, 1, 2001, value: 100m);

        b.PlumbingLink(custard, p1, 1500);
        b.PlumbingLink(custard, p2, 1000);

        var captured = new List<AttributionResult>();

        var sut = CreateSut(
            new[] { custard }.AsQueryable(),
            new[] { p1, p2 }.AsQueryable(),
            Enumerable.Empty<CornEntity>().AsQueryable(),
            Enumerable.Empty<CaliperEntity>().AsQueryable(),
            captured);

        List<Plumbing> input =
            [
                new Plumbing(p1.Id, p1.PhoneNumber, DateTimeOffset.FromUnixTimeSeconds(p1.UnixDate), Contents: null, Branch: null, MetaData: "null", p1.Source, [p1.PhoneNumber]),
                new Plumbing(p2.Id, p2.PhoneNumber, DateTimeOffset.FromUnixTimeSeconds(p2.UnixDate), Contents: null, Branch: null, MetaData: "null", p2.Source, [p2.PhoneNumber])
            ];

        var result = await sut.TransformAsync(input);

        Assert.True(result.IsSuccess);
        Assert.Single(captured);

        var expected = new AttributionResult
        {
            Custard = custard,
            MatchingPhone = custard.PhoneNumber.Number,
            FirstTouchUnixDate = 1000,
            Sand = sand,
            Source = AttributionSource.Plumbing,
            Entity = p1
        };

        AttributionResultAssert.Equivalent(expected, captured.Single());
    }

    [Fact]
    public async Task Plumbing_FirstTouch_With_Valid_Sand()
    {
        // Arrange
        var b = new AttributionTestBuilder();

        var custard = b.Custard(1, 2000);
        var plumbing = b.Plumbing(10, 1000);

        b.PlumbingLink(custard, plumbing, 1000);

        var sand = b.Sand(custard, 100, 3000, true, value: 123);

        var captured = new List<AttributionResult>();

        var sut = CreateSut(
            new[] { custard }.AsQueryable(),
            new[] { plumbing }.AsQueryable(),
            Enumerable.Empty<CornEntity>().AsQueryable(),
            Enumerable.Empty<CaliperEntity>().AsQueryable(),
            captured);

        List<Plumbing> input = [new Plumbing(plumbing.Id, plumbing.PhoneNumber, DateTimeOffset.FromUnixTimeSeconds(plumbing.UnixDate), Contents: null, Branch: null, MetaData: "null", plumbing.Source, [plumbing.PhoneNumber])];

        // Act
        var result = await sut.TransformAsync(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Single(captured);

        var expected = new AttributionResult
        {
            Custard = custard,
            MatchingPhone = custard.PhoneNumber.Number,
            FirstTouchUnixDate = 1000,
            Sand = sand,
            Source = AttributionSource.Plumbing,
            Entity = plumbing
        };

        AttributionResultAssert.Equivalent(expected, captured.Single());
    }

    [Fact]
    public async Task Plumbing_NotAttributable_When_Entity_After_Custard()
    {
        var b = new AttributionTestBuilder();

        var custard = b.Custard(1, 1000);
        var plumbing = b.Plumbing(10, 2000); // after custard

        b.PlumbingLink(custard, plumbing, 2000);
        b.Sand(custard, 1, 3000, true);

        var captured = new List<AttributionResult>();

        var sut = CreateSut(
            new[] { custard }.AsQueryable(),
            new[] { plumbing }.AsQueryable(),
            Enumerable.Empty<CornEntity>().AsQueryable(),
            Enumerable.Empty<CaliperEntity>().AsQueryable(),
            captured);

        var input = new List<Plumbing>
    {
        new Plumbing(plumbing.Id, plumbing.PhoneNumber, DateTimeOffset.FromUnixTimeSeconds(plumbing.UnixDate), null, null, "x", plumbing.Source, [plumbing.PhoneNumber])
    };

        var result = await sut.TransformAsync(input);

        Assert.True(result.IsSuccess);
        Assert.Empty(captured);
    }

    [Fact]
    public async Task Plumbing_NotAttributable_When_After_FirstCompletedSand()
    {
        var b = new AttributionTestBuilder();

        var custard = b.Custard(1, 5000);
        var plumbing = b.Plumbing(10, 4000);

        b.PlumbingLink(custard, plumbing, 4000);

        b.Sand(custard, 1, 3000, true); // completed sand BEFORE entity

        var captured = new List<AttributionResult>();

        var sut = CreateSut(
            new[] { custard }.AsQueryable(),
            new[] { plumbing }.AsQueryable(),
            Enumerable.Empty<CornEntity>().AsQueryable(),
            Enumerable.Empty<CaliperEntity>().AsQueryable(),
            captured);

        var input = new List<Plumbing>
    {
        new Plumbing(plumbing.Id, plumbing.PhoneNumber, DateTimeOffset.FromUnixTimeSeconds(plumbing.UnixDate), null, null, "x", plumbing.Source,[plumbing.PhoneNumber])
    };

        var result = await sut.TransformAsync(input);

        Assert.True(result.IsSuccess);
        Assert.Empty(captured);
    }

    [Fact]
    public async Task Plumbing_NotAttributable_When_Only_Incomplete_Sand()
    {
        var b = new AttributionTestBuilder();

        var custard = b.Custard(1, 5000);
        var plumbing = b.Plumbing(10, 1000);

        b.PlumbingLink(custard, plumbing, 1000);
        b.Sand(custard, 1, 3000, complete: false);

        var captured = new List<AttributionResult>();

        var sut = CreateSut(
            new[] { custard }.AsQueryable(),
            new[] { plumbing }.AsQueryable(),
            Enumerable.Empty<CornEntity>().AsQueryable(),
            Enumerable.Empty<CaliperEntity>().AsQueryable(),
            captured);

        var input = new List<Plumbing>
    {
        new Plumbing(plumbing.Id, plumbing.PhoneNumber, DateTimeOffset.FromUnixTimeSeconds(plumbing.UnixDate), null, null, "x", plumbing.Source,[plumbing.PhoneNumber])
    };

        var result = await sut.TransformAsync(input);

        Assert.True(result.IsSuccess);
        Assert.Empty(captured);
    }

    [Fact]
    public async Task Plumbing_MultipleSands_Uses_Earliest_Completed()
    {
        var b = new AttributionTestBuilder();

        var custard = b.Custard(1, 6000);
        var plumbing = b.Plumbing(10, 1000);

        b.PlumbingLink(custard, plumbing, 1000);

        var later = b.Sand(custard, 2, 5000, true, 50);
        var earlier = b.Sand(custard, 1, 3000, true, 100);

        var captured = new List<AttributionResult>();

        var sut = CreateSut(
            new[] { custard }.AsQueryable(),
            new[] { plumbing }.AsQueryable(),
            Enumerable.Empty<CornEntity>().AsQueryable(),
            Enumerable.Empty<CaliperEntity>().AsQueryable(),
            captured);

        var input = new List<Plumbing>
    {
        new Plumbing(plumbing.Id, plumbing.PhoneNumber, DateTimeOffset.FromUnixTimeSeconds(plumbing.UnixDate), null, null, "x", plumbing.Source,[plumbing.PhoneNumber])
    };

        var result = await sut.TransformAsync(input);

        Assert.Single(captured);

        var expected = new AttributionResult
        {
            Custard = custard,
            MatchingPhone = custard.PhoneNumber.Number,
            FirstTouchUnixDate = 1000,
            Sand = earlier,
            Source = AttributionSource.Plumbing,
            Entity = plumbing
        };

        AttributionResultAssert.Equivalent(expected, captured.Single());
    }

    [Fact]
    public async Task CrossEntity_Earlier_Corn_Wins()
    {
        var b = new AttributionTestBuilder();

        var custard = b.Custard(1, 5000);

        var plumbing = b.Plumbing(10, 2000);
        b.PlumbingLink(custard, plumbing, 2000);

        b.CornLink(custard, 1000, out var corn);

        b.Sand(custard, 1, 6000, true);

        var captured = new List<AttributionResult>();

        var sut = CreateSut(
            new[] { custard }.AsQueryable(),
            new[] { plumbing }.AsQueryable(),
            new[] { corn }.AsQueryable(),
            Enumerable.Empty<CaliperEntity>().AsQueryable(),
            captured);

        var input = new List<Plumbing>
    {
        new Plumbing(plumbing.Id, plumbing.PhoneNumber, DateTimeOffset.FromUnixTimeSeconds(plumbing.UnixDate), null, null, "x", plumbing.Source,[plumbing.PhoneNumber])
    };

        var result = await sut.TransformAsync(input);

        Assert.Single(captured);
        Assert.Equal(AttributionSource.Corn, captured.Single().Source);
    }

    [Fact]
    public async Task SamePhone_MultipleCustards_EarliestEffectiveWins()
    {
        var b = new AttributionTestBuilder();

        var c1 = b.Custard(1, 5000);
        var c2 = b.Custard(2, 4000);

        var plumbing = b.Plumbing(10, 1000);

        b.PlumbingLink(c1, plumbing, 1000);
        b.PlumbingLink(c2, plumbing, 1000);

        b.Sand(c1, 1, 6000, true);
        b.Sand(c2, 2, 4500, true);

        var captured = new List<AttributionResult>();

        var sut = CreateSut(
            new[] { c1, c2 }.AsQueryable(),
            new[] { plumbing }.AsQueryable(),
            Enumerable.Empty<CornEntity>().AsQueryable(),
            Enumerable.Empty<CaliperEntity>().AsQueryable(),
            captured);

        var input = new List<Plumbing>
    {
        new Plumbing(plumbing.Id, plumbing.PhoneNumber, DateTimeOffset.FromUnixTimeSeconds(plumbing.UnixDate), null, null, "x", plumbing.Source,[plumbing.PhoneNumber])
    };

        var result = await sut.TransformAsync(input);

        Assert.Single(captured);
        Assert.Equal(2, captured.Single().Custard.Id);
    }

    [Fact]
    public async Task SamePhone_SameEffectiveDate_BothReturned()
    {
        var b = new AttributionTestBuilder();

        var c1 = b.Custard(1, 5000);
        var c2 = b.Custard(2, 5000);

        var plumbing = b.Plumbing(10, 1000);

        b.PlumbingLink(c1, plumbing, id: 1);
        b.PlumbingLink(c2, plumbing, id: 2);

        b.Sand(c1, 1, 6000, true);
        b.Sand(c2, 2, 6000, true);

        var captured = new List<AttributionResult>();

        var sut = CreateSut(
            new[] { c1, c2 }.AsQueryable(),
            new[] { plumbing }.AsQueryable(),
            Enumerable.Empty<CornEntity>().AsQueryable(),
            Enumerable.Empty<CaliperEntity>().AsQueryable(),
            captured);

        var input = new List<Plumbing>
    {
        new Plumbing(plumbing.Id, plumbing.PhoneNumber, DateTimeOffset.FromUnixTimeSeconds(plumbing.UnixDate), null, null, "x", plumbing.Source,[plumbing.PhoneNumber])
    };

        var result = await sut.TransformAsync(input);

        Assert.Equal(2, captured.Count);
    }

}
