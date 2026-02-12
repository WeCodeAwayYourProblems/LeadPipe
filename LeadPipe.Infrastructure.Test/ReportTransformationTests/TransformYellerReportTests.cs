using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Data.Transform;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Infrastructure.Settings;
using NSubstitute;
using System.Linq.Expressions;

namespace LeadPipe.Infrastructure.Test.ReportTransformationTests;

public class TransformYellerReportTests
{
    #region Ctor and Fields
    private readonly IRepositoryFactory _repoFactory = Substitute.For<IRepositoryFactory>();
    private readonly IRepository<CustardEntity> _custardRepo = Substitute.For<IRepository<CustardEntity>>();
    private readonly IRepository<CaliperEntity> _caliperRepo = Substitute.For<IRepository<CaliperEntity>>();
    private readonly IRepository<CornEntity> _cornRepo = Substitute.For<IRepository<CornEntity>>();
    private readonly IRepository<PlumbingEntity> _plumbRepo = Substitute.For<IRepository<PlumbingEntity>>();

    private readonly IEntityToReport<PlumbingEntity, ReportYeller> _plumbToR = Substitute.For<IEntityToReport<PlumbingEntity, ReportYeller>>();
    private readonly IEntityToReport<CaliperEntity, ReportYeller> _caliperToR = Substitute.For<IEntityToReport<CaliperEntity, ReportYeller>>();
    private readonly IEntityToReport<CornEntity, ReportYeller> _cornToR = Substitute.For<IEntityToReport<CornEntity, ReportYeller>>();

    private readonly IEntityToReport<CustardPlumbingLink, ReportYeller> _cpLinkToR = Substitute.For<IEntityToReport<CustardPlumbingLink, ReportYeller>>();
    private readonly IEntityToReport<CustardCaliperLink, ReportYeller> _custCalToR = Substitute.For<IEntityToReport<CustardCaliperLink, ReportYeller>>();
    private readonly IEntityToReport<CustardCornLink, ReportYeller> _custCornToR = Substitute.For<IEntityToReport<CustardCornLink, ReportYeller>>();

    private readonly IYellerSettings _settings = Substitute.For<IYellerSettings>();

    public TransformYellerReportTests()
    {
        _repoFactory.GetRepository<CustardEntity>().Returns(_custardRepo);
        _repoFactory.GetRepository<CaliperEntity>().Returns(_caliperRepo);
        _repoFactory.GetRepository<CornEntity>().Returns(_cornRepo);
        _repoFactory.GetRepository<PlumbingEntity>().Returns(_plumbRepo);

        _settings.YellerCaliperSource1.Returns("Cal1");
        _settings.YellerCaliperSource2.Returns("Cal2");
        _settings.YellerCornSource.Returns("Corn1");
    }
    #endregion

    #region Helpers

    private static CustardEntity CreateCustard(
        long custardId,
        string phone,
        DateTime custardDate,
        List<(long entityId, DateTime entityDate)> calipers = null!,
        List<(long entityId, DateTime entityDate)> corns = null!,
        List<(long entityId, DateTime entityDate)> plumbs = null!,
        List<SandEntity>? sands = null
    )
    {
        return new CustardEntity
        {
            Id = custardId,
            PhoneNumber = new PhoneNumber(phone),
            Date = custardDate,
            SandEntities = sands ?? new List<SandEntity>(),
            CustardCaliperLinks = (calipers ?? new List<(long, DateTime)>())
                .Select(c => new CustardCaliperLink
                {
                    CustardId = custardId,
                    CaliperId = c.entityId,
                    MatchingPhone = long.Parse(phone),
                    UnixMatchDate = new DateTimeOffset(c.entityDate).ToUnixTimeSeconds()
                }).ToList(),
            CustardCornLinks = (corns ?? new List<(long, DateTime)>())
                .Select(c => new CustardCornLink
                {
                    CustardId = custardId,
                    CornId = c.entityId,
                    MatchingPhone = long.Parse(phone),
                    UnixMatchDate = new DateTimeOffset(c.entityDate).ToUnixTimeSeconds()
                }).ToList(),
            CustardPlumbingLinks = (plumbs ?? new List<(long, DateTime)>())
                .Select(c => new CustardPlumbingLink
                {
                    CustardId = custardId,
                    PlumbingId = c.entityId,
                    MatchingPhone = long.Parse(phone),
                    UnixMatchDate = new DateTimeOffset(c.entityDate).ToUnixTimeSeconds()
                }).ToList()
        };
    }

    private static CaliperEntity CreateCaliper(long id, string phone, DateTime date) =>
        new()
        {
            Id = id,
            PhoneNumber = new PhoneNumber(phone),
            Note = "note",
            Location = "loc",
            Source = "Cal1",
            Date = date
        };

    private static CornEntity CreateCorn(long id, string phone, DateTime date) =>
        new()
        {
            Id = id,
            PhoneNumber = new PhoneNumber(phone),
            Source = "Corn1",
            Date = date,
            UnixDate = new DateTimeOffset(date, TimeSpan.Zero).ToUnixTimeSeconds(),
            Payload = string.Empty,
            MetaData = string.Empty
        };

    private static PlumbingEntity CreatePlumbing(long id, string phone, DateTime date) =>
        new()
        {
            Id = id,
            PhoneNumber = new PhoneNumber(phone),
            MetaData = "meta",
            Source = Source.Yeller,
            Date = date
        };

    private static ReportYeller CreateReport(string phone) =>
        new()
        {
            event_id = "evt",
            event_time = 1,
            user_data = new UserData { ph = [phone] },
            custom_data = new CustomData { value = 1m, currency = "USD" }
        };

    #endregion

    [Fact]
    public async Task TransformAsync_WithNoData_ReturnsEmptyList()
    {
        // Arrange
        _custardRepo.FindWithDetailsAsync(Arg.Any<Expression<Func<CustardEntity, bool>>>())
            .Returns(Result.Success(new List<CustardEntity>()));

        _caliperRepo.FindAsync(Arg.Any<Expression<Func<CaliperEntity, bool>>>())
            .Returns(Result.Success(new List<CaliperEntity>()));

        _cornRepo.FindAsync(Arg.Any<Expression<Func<CornEntity, bool>>>())
            .Returns(Result.Success(new List<CornEntity>()));

        _plumbRepo.FindAsync(Arg.Any<Expression<Func<PlumbingEntity, bool>>>())
            .Returns(Result.Success(new List<PlumbingEntity>()));

        var transformer = new TransformYellerReport(
            _repoFactory, _cpLinkToR, _custCalToR, _custCornToR, _settings
        );

        // Act
        var result = await transformer.TransformAsync(new List<Plumbing>());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task TransformAsync_NoCustardLinks_ReturnsEmptyList()
    {
        // Arrange
        _custardRepo.FindWithDetailsAsync(Arg.Any<Expression<Func<CustardEntity, bool>>>())
            .Returns(Result.Success(new List<CustardEntity>()));

        _caliperRepo.FindAsync(Arg.Any<Expression<Func<CaliperEntity, bool>>>())
            .Returns(Result.Success(new List<CaliperEntity>()));
        _cornRepo.FindAsync(Arg.Any<Expression<Func<CornEntity, bool>>>())
            .Returns(Result.Success(new List<CornEntity>()));
        _plumbRepo.FindAsync(Arg.Any<Expression<Func<PlumbingEntity, bool>>>())
            .Returns(Result.Success(new List<PlumbingEntity>()));

        var transformer = new TransformYellerReport(
            _repoFactory, _cpLinkToR, _custCalToR, _custCornToR, _settings
        );

        // Act
        var result = await transformer.TransformAsync(new List<Plumbing>());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task TransformAsync_CustardWithMultipleCaliperLinks_EarliestAttributableWins()
    {
        // Arrange
        long numb = 5551234567;

        var custard = new CustardEntity
        {
            Id = 1,
            PhoneNumber = new PhoneNumber(numb),
            Date = new DateTime(2026, 1, 1),
            SandEntities = [],
            CustardCaliperLinks =
            [
                new CustardCaliperLink { CustardId = 1, CaliperId = 1, MatchingPhone = numb, UnixMatchDate = 1000 },
                new CustardCaliperLink { CustardId = 1, CaliperId = 2, MatchingPhone = numb, UnixMatchDate = 1001 }
            ]
        };

        List<CaliperEntity> calipers =
        [
            new CaliperEntity()
            {
                Id = 1,
                PhoneNumber = new PhoneNumber(numb),
                Source = "Cal1",
                Date = new DateTime(2025, 12, 31), // EARLIEST
                Note = "Note",
                Location = "Location"
            },
            new CaliperEntity()
            {
                Id = 2,
                PhoneNumber = new PhoneNumber(numb),
                Source = "Cal1",
                Date = new DateTime(2026, 1, 2),
                Note = "Note",
                Location = "Location"
            }
        ];

        _custardRepo.FindWithDetailsAsync(Arg.Any<Expression<Func<CustardEntity, bool>>>())
            .Returns(Result.Success(new List<CustardEntity> { custard }));

        _caliperRepo.FindAsync(Arg.Any<Expression<Func<CaliperEntity, bool>>>())
            .Returns(Result.Success(calipers));

        _cornRepo.FindAsync(Arg.Any<Expression<Func<CornEntity, bool>>>())
            .Returns(Result.Success(new List<CornEntity>()));

        _plumbRepo.FindAsync(Arg.Any<Expression<Func<PlumbingEntity, bool>>>())
            .Returns(Result.Success(new List<PlumbingEntity>()));

        _custCalToR.Translate(Arg.Any<CustardCaliperLink>())
            .Returns(call =>
            {
                var link = call.Arg<CustardCaliperLink>();
                long phone = link.MatchingPhone;
                return new ReportYeller
                {
                    event_time = link.UnixMatchDate,
                    event_id = $"caliper-{link.CaliperId}",
                    user_data = new UserData() { ph = [phone.ToString()] },
                    custom_data = new CustomData() { currency = "USD", value = 1m }
                };
            });

        _caliperToR.Translate(Arg.Any<CaliperEntity>())
            .Returns(c =>
            {
                var e = c.Arg<CaliperEntity>();
                long phone = e.PhoneNumber.Number;
                long date = e.UnixDate;
                return new ReportYeller()
                {
                    event_time = date,
                    event_id = $"entity-{e.Id}",
                    user_data = new UserData() { ph = [phone.ToString()] },
                    custom_data = new CustomData() { currency = "USD", value = 1m }
                };
            });

        var transformer = new TransformYellerReport(
            _repoFactory,
            _cpLinkToR,
            _custCalToR,
            _custCornToR,
            _settings
        );

        // Act
        var result = await transformer.TransformAsync([]);

        // Assert
        Assert.True(result.IsSuccess);

        var reports = result.Value;

        string cExpected = "caliper-1";

        List<ReportYeller> cReport = [.. reports.Where(r => r.event_id == cExpected)];

        Assert.Single(reports);
        Assert.Equal(cExpected, cReport[0].event_id);

    }

    [Fact]
    public async Task TransformAsync_SandDate_PreventsAttributable()
    {
        // Arrange
        long numb = 5551234567;
        var custard = new CustardEntity
        {
            PhoneNumber = new PhoneNumber(numb),
            Id = 1,
            Date = new DateTime(2026, 1, 1),
            SandEntities = new List<SandEntity>
            {
                new SandEntity { Id=1, CustardId=1, Offerman="1000" }
            },
            CustardCaliperLinks = new List<CustardCaliperLink>
            {
                new CustardCaliperLink { CustardId=1, CaliperId=1, MatchingPhone=numb, UnixMatchDate=1000 }
            }
        };
        var calipers = new List<CaliperEntity>
        {
            new CaliperEntity
            {
                Id = 1,
                PhoneNumber = new PhoneNumber(numb),
                Note = "n1",
                Location = "loc1",
                Source = "Cal1",
                Date = new DateTime(2026,1,2)
            }
        };

        _custardRepo.FindWithDetailsAsync(Arg.Any<Expression<Func<CustardEntity, bool>>>())
            .Returns(Result.Success(new List<CustardEntity> { custard }));
        _caliperRepo.FindAsync(Arg.Any<Expression<Func<CaliperEntity, bool>>>())
            .Returns(Result.Success(calipers));
        _cornRepo.FindAsync(Arg.Any<Expression<Func<CornEntity, bool>>>())
            .Returns(Result.Success(new List<CornEntity>()));
        _plumbRepo.FindAsync(Arg.Any<Expression<Func<PlumbingEntity, bool>>>())
            .Returns(Result.Success(new List<PlumbingEntity>()));

        _caliperToR.Translate(Arg.Any<CaliperEntity>())
            .Returns(ci => new ReportYeller
            {
                event_id = "evt",
                event_time = 1,
                user_data = new UserData { ph = new[] { $"{numb}" } },
                custom_data = new CustomData { value = 1m, currency = "USD" }
            });

        var transformer = new TransformYellerReport(
            _repoFactory, _cpLinkToR, _custCalToR, _custCornToR, _settings
        );

        // Act
        await transformer.TransformAsync(new List<Plumbing>());

        // Only custard links are translated
        _custCalToR.Received(1).Translate(Arg.Any<CustardCaliperLink>());
        _caliperToR.DidNotReceive().Translate(Arg.Any<CaliperEntity>());

    }

    [Fact]
    public async Task CustardAttribution_DoesNotSuppress_NonCustardEntities()
    {
        long numb = 5551111111;

        var custard = new CustardEntity
        {
            Id = 1,
            PhoneNumber = new PhoneNumber(numb),
            Date = new DateTime(2026, 1, 1),
            SandEntities = [],
            CustardCaliperLinks =
            [
                new CustardCaliperLink
            {
                CustardId = 1,
                CaliperId = 1,
                MatchingPhone = numb,
                UnixMatchDate = 1000
            }
            ]
        };

        List<CaliperEntity> calipers =
        [
            new CaliperEntity { Id = 1, PhoneNumber = new PhoneNumber(numb), Source="Cal1", Date = new DateTime(2025,12,31), Note = "Note", Location = "Location" }, // custard
            new CaliperEntity { Id = 2, PhoneNumber = new PhoneNumber(2345678950), Source="Cal1", Date = new DateTime(2026,1,2), Note = "Note", Location="Location" }   // non-custard
        ];

        _custardRepo.FindWithDetailsAsync(Arg.Any<Expression<Func<CustardEntity, bool>>>())
            .Returns(Result.Success<List<CustardEntity>>([custard]));

        _caliperRepo.FindAsync(Arg.Any<Expression<Func<CaliperEntity, bool>>>())
            .Returns(Result.Success(calipers));

        _cornRepo.FindAsync(Arg.Any<Expression<Func<CornEntity, bool>>>())
            .Returns(Result.Success<List<CornEntity>>([]));

        _plumbRepo.FindAsync(Arg.Any<Expression<Func<PlumbingEntity, bool>>>())
            .Returns(Result.Success<List<PlumbingEntity>>([]));

        var transformer = new TransformYellerReport(
            _repoFactory, _cpLinkToR, _custCalToR, _custCornToR, _settings
        );

        await transformer.TransformAsync([]);

        _custCalToR.Received(1).Translate(Arg.Any<CustardCaliperLink>());
        _caliperToR.Received(1).Translate(Arg.Is<CaliperEntity>(c => c.Id == 2));
    }

    [Fact]
    public async Task SandBlocksAttribution_ButFallsBackToEntityTranslation()
    {
        long numb = 5552222222;

        var custard = new CustardEntity
        {
            Id = 1,
            PhoneNumber = new PhoneNumber(numb),
            Date = new DateTime(2026, 1, 1),
            SandEntities =
            [
                new SandEntity { Id = 1, CustardId = 1, Offerman = "1000" }
            ],
            CustardCaliperLinks =
            [
                new CustardCaliperLink
            {
                CustardId = 1,
                CaliperId = 1,
                MatchingPhone = numb,
                UnixMatchDate = 1000
            }
            ]
        };

        List<CaliperEntity> calipers =
        [
            new CaliperEntity
        {
            Id = 1,
            PhoneNumber = new PhoneNumber(numb),
            Source = "Cal1",
            Date = new DateTime(2026, 1, 2), // after sand
            Note = "Note",
            Location = "Location"
        }
        ];

        _custardRepo.FindWithDetailsAsync(Arg.Any<Expression<Func<CustardEntity, bool>>>())
            .Returns(Result.Success<List<CustardEntity>>([custard]));

        _caliperRepo.FindAsync(Arg.Any<Expression<Func<CaliperEntity, bool>>>())
            .Returns(Result.Success(calipers));

        _cornRepo.FindAsync(Arg.Any<Expression<Func<CornEntity, bool>>>())
            .Returns(Result.Success<List<CornEntity>>([]));

        _plumbRepo.FindAsync(Arg.Any<Expression<Func<PlumbingEntity, bool>>>())
            .Returns(Result.Success<List<PlumbingEntity>>([]));

        var transformer = new TransformYellerReport(
            _repoFactory, _cpLinkToR, _custCalToR, _custCornToR, _settings
        );

        await transformer.TransformAsync(new List<Plumbing>());

        _custCalToR.DidNotReceive().Translate(Arg.Any<CustardCaliperLink>());
        _caliperToR.DidNotReceive().Translate(Arg.Any<CaliperEntity>());

    }

    [Fact]
    public async Task Attribution_Is_Isolated_PerEntityType()
    {
        long numb = 5553333333;

        var custard = new CustardEntity
        {
            Id = 1,
            PhoneNumber = new PhoneNumber(numb),
            Date = new DateTime(2026, 1, 1),
            SandEntities = [],
            CustardCaliperLinks =
            [
                new CustardCaliperLink
            {
                CustardId = 1,
                CaliperId = 1,
                MatchingPhone = numb,
                UnixMatchDate = 1000
            }
            ]
        };

        List<CaliperEntity> calipers =
        [
            new CaliperEntity
            {
                Id = 1,
                PhoneNumber = new PhoneNumber(numb),
                Source="Cal1",
                Date = new DateTime(2025,12,31),
                Note = "Note",
                Location = "Location"
            }
        ];

        List<CornEntity> corns =
        [
            new CornEntity
            {
                Id = 1,
                PhoneNumber = new PhoneNumber(numb),
                Source="Corn1",
                Date = new DateTime(2026,1,2),
                UnixDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Payload = "payload",
                MetaData = "metadata"
            }
        ];

        _custardRepo.FindWithDetailsAsync(Arg.Any<Expression<Func<CustardEntity, bool>>>())
            .Returns(Result.Success<List<CustardEntity>>([custard]));

        _caliperRepo.FindAsync(Arg.Any<Expression<Func<CaliperEntity, bool>>>())
            .Returns(Result.Success(calipers));

        _cornRepo.FindAsync(Arg.Any<Expression<Func<CornEntity, bool>>>())
            .Returns(Result.Success(corns));

        _plumbRepo.FindAsync(Arg.Any<Expression<Func<PlumbingEntity, bool>>>())
            .Returns(Result.Success<List<PlumbingEntity>>([]));

        var transformer = new TransformYellerReport(
            _repoFactory, _cpLinkToR, _custCalToR, _custCornToR, _settings
        );

        await transformer.TransformAsync([]);

        _custCalToR.Received(1).Translate(Arg.Any<CustardCaliperLink>());
        _cornToR.Received(1).Translate(Arg.Any<CornEntity>());
    }

    #region Composed

    [Fact]
    public async Task TransformAsync_ComposedScenario_ReturnsExpectedShape()
    {
        // Arrange
        // Custard 1: has Caliper 1 (before sand), Corn 1 (after sand)
        long numb1 = 5551000001;
        long numb2 = 5551000002;
        long numb3 = 5551000000;
        var custard1 = CreateCustard(
            custardId: 1,
            phone: $"{numb1}",
            custardDate: new DateTime(2026, 1, 1),
            calipers: new List<(long, DateTime)> { (1, new DateTime(2025, 12, 31)) },
            corns: new List<(long, DateTime)> { (1, new DateTime(2026, 1, 2)) },
            plumbs: new List<(long, DateTime)> { (1, new DateTime(2025, 12, 30)) },
            sands: new List<SandEntity>
            {
                new SandEntity { Id = 1, CustardId=1, Offerman="1000" },
                new SandEntity { Id = 2, CustardId=1, Offerman="1001" }
            }
        );

        var custard2 = CreateCustard(
            custardId: 2,
            phone: $"{numb2}",
            custardDate: new DateTime(2026, 2, 1),
            calipers: new List<(long, DateTime)> { (2, new DateTime(2026, 1, 31)) }
        );

        var calipers = new List<CaliperEntity>
        {
            CreateCaliper(1, $"{numb1}", new DateTime(2025,12,31)),
            CreateCaliper(2, $"{numb2}", new DateTime(2026,1,31)),
            CreateCaliper(3, $"{numb3}", new DateTime(2026,2,1)) // Non-custard
        };

        var corns = new List<CornEntity>
        {
            CreateCorn(1, $"{numb1}", new DateTime(2026,1,2)),
            CreateCorn(2, $"{numb2}", new DateTime(2026,2,2)) // Non-custard
        };

        var plumbs = new List<PlumbingEntity>
        {
            CreatePlumbing(1, $"{numb1}", new DateTime(2025,12,30)),
            CreatePlumbing(2, $"{numb2}", new DateTime(2026,2,2)) // Non-custard
        };

        _custardRepo.FindWithDetailsAsync(Arg.Any<Expression<Func<CustardEntity, bool>>>())
            .Returns(Result.Success(new List<CustardEntity> { custard1, custard2 }));

        _caliperRepo.FindAsync(Arg.Any<Expression<Func<CaliperEntity, bool>>>())
            .Returns(Result.Success(calipers));
        _cornRepo.FindAsync(Arg.Any<Expression<Func<CornEntity, bool>>>())
            .Returns(Result.Success(corns));
        _plumbRepo.FindAsync(Arg.Any<Expression<Func<PlumbingEntity, bool>>>())
            .Returns(Result.Success(plumbs));

        _caliperToR.Translate(Arg.Any<CaliperEntity>()).Returns(_ => CreateReport("entity"));
        _cornToR.Translate(Arg.Any<CornEntity>()).Returns(_ => CreateReport("entity"));
        _plumbToR.Translate(Arg.Any<PlumbingEntity>()).Returns(_ => CreateReport("entity"));

        _custCalToR.Translate(Arg.Any<CustardCaliperLink>()).Returns(_ => CreateReport("custard"));
        _custCornToR.Translate(Arg.Any<CustardCornLink>()).Returns(_ => CreateReport("custard"));
        _cpLinkToR.Translate(Arg.Any<CustardPlumbingLink>()).Returns(_ => CreateReport("custard"));

        var transformer = new TransformYellerReport(
            _repoFactory, _cpLinkToR, _custCalToR, _custCornToR, _settings
        );

        var inputPlumbings = new List<Plumbing>
        {
            new Plumbing(1, new PhoneNumber(numb1), DateTimeOffset.Now, null, null, "meta", Source.Yeller)
        };

        // Act
        var result = await transformer.TransformAsync(inputPlumbings);

        // Assert
        Assert.True(result.IsSuccess);

        // Custard 1: Caliper 1 attributable, Corn 1 non-attributable, Plumbing 1 attributable
        // Custard 2: Caliper 2 attributable
        // Non-custard entities (Caliper 3, Corn 2, Plumbing 2) are ignored

        // Expected: 4 reports
        Assert.Equal(4, result.Value.Count);

    }

    #endregion

}
