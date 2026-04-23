using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Infrastructure.Settings;
using LeadPipe.Translation.Primitives;
using LeadPipe.Translation.Translate.EntityToVo;
using LeadPipe.Translation.Translate.VoToEntity;
using NSubstitute;

namespace LeadPipe.Translation.Test;

public class TranslatorBackAndForthFullTests
{
    private readonly IDateTimeTranslate _dt = Substitute.For<IDateTimeTranslate>();
    private readonly IInfrastructureSettings _settings = Substitute.For<IInfrastructureSettings>();
    private readonly IEntityToVo<CustardEntity, Custard> _eToCustard = Substitute.For<IEntityToVo<CustardEntity, Custard>>();

    private static TVo RoundTrip<TEntity1, TEntity2, TVo>(
        TEntity1 entity,
        IEntityToVo<TEntity1, TVo> e1ToVo,
        IVoToEntity<TVo, TEntity2> voToEntity2,
        IEntityToVo<TEntity2, TVo> e2ToVo,
        int iterations = 1_000_000
    )
    {
        // Generate the vo once 
        TVo vo = e1ToVo.Translate(entity);
        for (int i = 0; i < iterations; i++)
        {
            // Translate the entity 1 to Vo
            vo = e1ToVo.Translate(entity);

            // Translate the vo to entity 2
            TEntity2? e2 = voToEntity2.Translate(vo);

            // Translate the entity2 back to vo
            vo = e2ToVo.Translate(e2);
        }
        return vo;
    }

    #region CaliperEntity ↔ Caliper

    [Fact]
    public void CaliperEntity_RoundTrip_Idempotent()
    {
        var entity = new CaliperEntity
        {
            Id = 1,
            PhoneNumber = new(5551234567),
            Date = new DateTime(2025, 6, 1, 12, 0, 0),
            Duration = 120,
            Note = "Note",
            Source = "Source",
            Label = "Label",
            Location = "Location",
            Billable = true
        };

        IEntityToVo<CaliperMySqlEntity, Caliper> entity2ToVo = new CaliperMySqlEntityToCaliper();
        IEntityToVo<CaliperEntity, Caliper> toVo = new CaliperEntityToCaliper();
        IVoToEntity<Caliper, CaliperMySqlEntity> voToEntity2 = new CaliperToCaliperMySqlEntity();

        Caliper vo = RoundTrip(entity, toVo, voToEntity2, entity2ToVo, 1_000_000);

        Assert.Equal(entity.Id, vo.Id);
        Assert.Equal(entity.PhoneNumber.Number, vo.Number.Number);
        Assert.Equal(TimeSpan.FromSeconds(entity.Duration), vo.Duration);
        Assert.Equal(entity.Note, vo.Note);
        Assert.Equal(entity.Source, vo.Source);
        Assert.Equal(TimeSpan.Zero, vo.Date.Offset);
    }

    #endregion

    #region CaliperMySqlEntity ↔ Caliper

    [Fact]
    public void CaliperMySqlEntity_RoundTrip_Idempotent()
    {
        var entity = new CaliperMySqlEntity
        {
            call_id = 1,
            contact_number_clean = "5551234567",
            called_at_utc = new DateTime(2025, 6, 1, 12, 0, 0),
            duration = 90,
            sale_billable = "billable",
            source = "Src",
            location = "Loc",
            transcriptions = [new() { transcription = "Hello" }],
            summaries = [new() { summary = "Summary" }]
        };

        IEntityToVo<CaliperMySqlEntity, Caliper> toVo = new CaliperMySqlEntityToCaliper();
        IEntityToVo<CaliperEntity, Caliper> entity2ToVo = new CaliperEntityToCaliper();
        IVoToEntity<Caliper, CaliperEntity> toEntity = new CaliperToCaliperEntity();

        Caliper vo = RoundTrip(entity, toVo, toEntity, entity2ToVo, 500_000);

        Assert.Equal(entity.call_id, vo.Id);
        Assert.Equal(5551234567, vo.Number.Number);
        Assert.Equal(TimeSpan.FromSeconds((double)entity.duration!), vo.Duration);
        Assert.Equal("Summary | Hello", vo.Note);
        Assert.Equal(TimeSpan.Zero, vo.Date.Offset);
    }

    #endregion

    #region CornMySqlEntity ↔ CornFormula

    [Fact]
    public void CornMySqlEntity_RoundTrip_Idempotent()
    {
        var entity = new CornMySqlEntity
        {
            id = 1,
            phoneNumber = "5551002000",
            timestamp = new DateTime(2025, 6, 1, 12, 0, 0),
            comments = "Payload",
            form = "FormX",
            referringURL = "http://ref.url",
            source = "Source"
        };

        IEntityToVo<CornMySqlEntity, CornFormula> toVo = new CornMySqlEntityToCornFormula(_settings);
        IEntityToVo<CornEntity, CornFormula> toVo2 = new CornEntityToCornFormula();
        IVoToEntity<CornFormula, CornEntity> toEntity = new CornFormulaToCornEntity();

        CornFormula vo = RoundTrip(entity, toVo, toEntity, toVo2, 500_000);

        Assert.Equal(entity.id, vo.Id);
        Assert.Equal(5551002000, vo.PhoneNumber.Number);
        Assert.Equal("Payload", vo.PayLoad);
        Assert.Equal("Form: FormX | Referring: http://ref.url", vo.MetaData);
        Assert.Equal(TimeSpan.Zero, vo.Date.Offset);
    }

    #endregion

    #region CustardMySqlEntity ↔ Custard
    private readonly IDateTimeTranslate dt = Substitute.For<IDateTimeTranslate>();
    [Fact]
    public void CustardMySqlEntity_RoundTrip_Idempotent()
    {
        var entity = new CustardMySqlEntity
        {
            customerID = 10,
            phone1 = "5551002000",
            phone2 = "5551003000",
            status = 1,
            dateAdded = new DateTime(2025, 6, 1, 12, 0, 0),
            dateCancelled = new DateTime(2025, 12, 31, 23, 59, 59)
        };

        IEntityToVo<CustardMySqlEntity, Custard> toVo = new CustardMySqlEntityToCustard(dt);
        IEntityToVo<CustardEntity, Custard> toVo2 = new CustardEntityToCustard();
        IVoToEntity<Custard, CustardEntity> toEntity = new CustardToCustardEntity();

        Custard vo = RoundTrip(entity, toVo, toEntity, toVo2, 500_000);

        Assert.Equal(entity.customerID, vo.Id);
        Assert.True(vo.Status);
        Assert.Equal(5551002000, vo.Phone1.Number);
        Assert.Equal(5551003000, vo.Phone2?.Number);
        Assert.Equal(TimeSpan.Zero, vo.Date.Offset);
        Assert.Equal(TimeSpan.Zero, vo.DateCancelled?.Offset);
    }

    #endregion

    #region PlumbingEntity ↔ Plumbing

    [Fact]
    public void PlumbingEntity_RoundTrip_Idempotent()
    {
        var entity = new PlumbingEntity
        {
            Id = 5,
            PhoneNumber = new(5551112222),
            Date = new DateTime(2025, 6, 1, 12, 0, 0),
            Contents = "Some content",
            MetaData = "Meta",
            Source = Source.Test
        };

        IEntityToVo<PlumbingEntity, Plumbing> toVo = new PlumbingEntityToPlumbing();
        IVoToEntity<Plumbing, PlumbingEntity> toEntity = new PlumbingToPlumbingEntity();

        Plumbing vo = RoundTrip(entity, toVo, toEntity, toVo, 500_000);

        Assert.Equal(entity.Id, vo.Id);
        Assert.Equal(entity.PhoneNumber.Number, vo.PhoneNumber.Number);
        Assert.Equal("Some content", vo.Contents);
        Assert.Equal(TimeSpan.Zero, vo.Date.Offset);
    }

    #endregion

    #region SandMySqlEntity ↔ Sandwich

    [Fact]
    public void SandMySqlEntity_RoundTrip_Idempotent()
    {
        _dt.Convert(Arg.Any<DateTime>(), Arg.Any<ETimeZone>())
           .Returns(callInfo => new DateTimeOffset((DateTime)callInfo[0], TimeSpan.Zero));

        var custardVo = new Custard(
            Id: 10,
            Status: true,
            Phone1: new PhoneNumber(5551002000),
            Phone2: new PhoneNumber(5551003000),
            Date: new DateTimeOffset(2025, 6, 1, 12, 0, 0, TimeSpan.Zero),
            DateCancelled: new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero)
        );

        var sandwichVo = new Sandwich(
            SandId: 99,
            CustardId: 10,
            Custard: custardVo,
            Date: new DateTimeOffset(2025, 6, 1, 12, 0, 0, TimeSpan.Zero),
            DateCancelled: new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero),
            Active: true,
            Complete: true,
            Type: "Premium",
            Value: 199.99m,
            Seller: 2,
            Seller2: 3,
            Seller3: 4,
            Offerman: "Offerman"
        );

        var toEntity = new SandwichToSandEntity();
        var toVo = new SandEntityToSandwich(_eToCustard);

        Sandwich result = sandwichVo;
        for (int i = 0; i < 500_000; i++)
        {
            SandEntity entity = toEntity.Translate(result);
            result = toVo.Translate(entity);
        }

        Assert.Equal(sandwichVo.SandId, result.SandId);
        Assert.Equal(sandwichVo.Custard.Phone1.Number, result.Custard.Phone1.Number);
        Assert.Equal(TimeSpan.Zero, result.Date.Offset);
        Assert.Equal(TimeSpan.Zero, result.DateCancelled?.Offset);
    }

    #endregion

    #region SandEntity ↔ Sandwich

    [Fact]
    public void SandEntity_RoundTrip_Idempotent()
    {
        _dt.Convert(Arg.Any<DateTime>(), Arg.Any<ETimeZone>())
           .Returns(callInfo => new DateTimeOffset((DateTime)callInfo[0], TimeSpan.Zero));

        var dt = new DateTime(2025, 6, 1, 12, 0, 0);
        var d = new DateTimeOffset(dt);
        var cxl = new DateTime(2025, 12, 31, 23, 59, 59);
        var c = new DateTimeOffset(cxl);
        var custardEntity = new CustardEntity
        {
            Id = 10,
            Active = true,
            PhoneNumber = new(5551002000),
            PhoneNumber2 = new(5551003000),
            Date = dt,
            UnixDate = d.ToUnixTimeMilliseconds(),
            UnixCancelDate = c.ToUnixTimeSeconds()
        };

        var entity = new SandEntity
        {
            Id = 99,
            CustardId = 10,
            CustardEntity = custardEntity,
            Date = dt,
            UnixDate = d.ToUnixTimeSeconds(),
            UnixCancelDate = c.ToUnixTimeSeconds(),
            Active = true,
            Complete = true,
            Type = "Premium",
            Value = 199.99m,
            Seller = 2,
            Seller2 = 3,
            Seller3 = 4,
            Offerman = string.Empty
        };

        var toVo = new SandEntityToSandwich(_eToCustard);
        var toEntity = new SandwichToSandEntity();

        Sandwich vo = RoundTrip(entity, toVo, toEntity, toVo, 500_000);

        Assert.Equal(entity.Id, vo.SandId);
        Assert.Equal(entity.CustardEntity.PhoneNumber.Number, vo.Custard.Phone1.Number);
        Assert.Equal(TimeSpan.Zero, vo.Date.Offset);
        Assert.Equal(TimeSpan.Zero, vo.DateCancelled?.Offset);
    }

    #endregion
}
