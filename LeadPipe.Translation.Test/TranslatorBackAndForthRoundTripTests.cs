using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Infrastructure.Settings;
using LeadPipe.Translation.Primitives;
using LeadPipe.Translation.Translate.EntityToVo;
using LeadPipe.Translation.Translate.VoToEntity;
using NSubstitute;

namespace LeadPipe.Translation.Test;

public class TranslatorBackAndForthRoundTripTests
{
    private readonly IDateTimeTranslate _dt = Substitute.For<IDateTimeTranslate>();
    private readonly IInfrastructureSettings _settings = Substitute.For<IInfrastructureSettings>();

    // Utility for repeated round-trip
    private static TVo RoundTrip<TEntity1, TEntity2, TVo>(
        TEntity1 e1,
        IEntityToVo<TEntity1, TVo> e1ToVo,
        IVoToEntity<TVo, TEntity2> voToE2,
        IEntityToVo<TEntity2, TVo> e2ToVo,
        IVoToEntity<TVo, TEntity1> voToE1,
        int iterations = 1_000
    )
    {
        TVo vo = e1ToVo.Translate(e1);
        for (int i = 0; i < iterations; i++)
        {
            vo = e1ToVo.Translate(e1);
            TEntity2? e2 = voToE2.Translate(vo);
            vo = e2ToVo.Translate(e2);
            e1 = voToE1.Translate(vo);
        }
        return vo;
    }

    [Fact]
    public void CaliperEntity_To_Caliper_RoundTrip_ShouldRemainConsistent()
    {
        var e1 = new CaliperEntity
        {
            Id = 1,
            PhoneNumber = new(5551234567),
            Date = new DateTime(2025, 6, 1, 12, 0, 0),
            Duration = 123,
            Note = "Note",
            Source = "Source",
            Location = "Location",
            Billable = true
        };

        IEntityToVo<CaliperEntity, Caliper> e1ToVo = new CaliperEntityToCaliper();
        IEntityToVo<CaliperMySqlEntity, Caliper> e2ToVo = new CaliperMySqlEntityToCaliper();
        IVoToEntity<Caliper, CaliperMySqlEntity> voToE2 = new CaliperToCaliperMySqlEntity();
        IVoToEntity<Caliper, CaliperEntity> voToE1 = new CaliperToCaliperEntity();

        Caliper vo = RoundTrip(e1, e1ToVo, voToE2, e2ToVo, voToE1, 1_000_000);

        Assert.Equal(e1.Id, vo.Id);
        Assert.Equal(e1.PhoneNumber.Number, vo.Number.Number);
        Assert.Equal(TimeSpan.FromSeconds(e1.Duration), vo.Duration);
        Assert.Equal(e1.Note, vo.Note);
        Assert.Equal(TimeSpan.Zero, vo.Date.Offset);
    }

    [Fact]
    public void CaliperMySqlEntity_To_Caliper_RoundTrip_ShouldRemainConsistent()
    {
        long numb = 5551234567;
        var e1 = new CaliperMySqlEntity
        {
            call_id = 1,
            contact_number_clean = $"{numb}",
            called_at_utc = new DateTime(2025, 6, 1, 12, 0, 0),
            duration = 123,
            sale_billable = "billable",
            source = "Source",
            location = "Location",
            transcriptions = new List<TranscriptionMySqlEntity> { new() { transcription = "Hello" } },
            summaries = new List<SummaryMySqlEntity> { new() { summary = "Summary" } }
        };

        IEntityToVo<CaliperMySqlEntity, Caliper> e1ToVo = new CaliperMySqlEntityToCaliper();
        IEntityToVo<CaliperEntity, Caliper> e2ToVo = new CaliperEntityToCaliper();
        IVoToEntity<Caliper, CaliperMySqlEntity> voToE1 = new CaliperToCaliperMySqlEntity();
        IVoToEntity<Caliper, CaliperEntity> voToE2 = new CaliperToCaliperEntity();

        Caliper vo = RoundTrip(e1, e1ToVo, voToE2, e2ToVo, voToE1, 1_000_000);

        Assert.Equal(e1.call_id, vo.Id);
        Assert.Equal(numb, vo.Number.Number);
        Assert.Equal(TimeSpan.FromSeconds((double)e1.duration), vo.Duration);
        Assert.Equal("Summary | Hello", vo.Note);
        Assert.Equal(TimeSpan.Zero, vo.Date.Offset);
    }

    [Fact]
    public void CornMySqlEntity_To_CornFormula_RoundTrip_ShouldRemainConsistent()
    {
        long numb = 5551002000;
        var e1 = new CornMySqlEntity
        {
            id = 1,
            phoneNumber = $"{numb}",
            timestamp = new DateTime(2025, 6, 1, 12, 0, 0),
            comments = "Payload",
            form = "FormX",
            referringURL = "http://ref.url",
            source = "Source"
        };

        IEntityToVo<CornMySqlEntity, CornFormula> e1ToVo = new CornMySqlEntityToCornFormula(_settings);
        IEntityToVo<CornEntity, CornFormula> e2ToVo = new CornEntityToCornFormula();
        IVoToEntity<CornFormula, CornMySqlEntity> voToE1 = new CornFormulaToCornMySqlEntity();
        IVoToEntity<CornFormula, CornEntity> voToE2 = new CornFormulaToCornEntity();

        CornFormula vo = RoundTrip(e1, e1ToVo, voToE2, e2ToVo, voToE1, 1_000_000);

        Assert.Equal(e1.id, vo.Id);
        Assert.Equal(numb, vo.PhoneNumber.Number);
        Assert.Equal("Payload", vo.PayLoad);
        Assert.Equal("Form: FormX | Referring: http://ref.url", vo.MetaData);
        Assert.Equal(TimeSpan.Zero, vo.Date.Offset);
    }

    [Fact]
    public void CustardMySqlEntity_To_Custard_RoundTrip_ShouldRemainConsistent()
    {
        long numb1 = 5551002000;
        long numb2 = 5551003000;
        var e1 = new CustardMySqlEntity
        {
            customerID = 10,
            phone1 = $"{numb1}",
            phone2 = $"{numb2}",
            status = 1,
            dateAdded = new DateTime(2025, 6, 1, 12, 0, 0),
            dateCancelled = new DateTime(2025, 12, 31, 23, 59, 59)
        };

        IEntityToVo<CustardMySqlEntity, Custard> e1ToVo = new CustardMySqlEntityToCustard(_dt);
        IEntityToVo<CustardEntity, Custard> e2ToVo = new CustardEntityToCustard();
        IVoToEntity<Custard, CustardMySqlEntity> voToE1 = new CustardToCustardMySqlEntity();
        IVoToEntity<Custard, CustardEntity> voToE2 = new CustardToCustardEntity();

        Custard vo = RoundTrip(e1, e1ToVo, voToE2, e2ToVo, voToE1, 1_000_000);

        Assert.Equal(e1.customerID, vo.Id);
        Assert.True(vo.Status);
        Assert.Equal(numb1, vo.Phone1.Number);
        Assert.Equal(numb2, vo.Phone2?.Number);
        Assert.Equal(TimeSpan.Zero, vo.Date.Offset);
        Assert.Equal(TimeSpan.Zero, vo.DateCancelled.Offset);
    }

    [Fact]
    public void Sandwich_To_SandEntity_RoundTrip_ShouldRemainConsistent()
    {
        _dt.Convert(Arg.Any<DateTime>(), Arg.Any<ETimeZone>()).Returns(callInfo => new DateTimeOffset((DateTime)callInfo[0], TimeSpan.Zero));

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
            Offerman: "0"
        );

        var toEntity = new SandwichToSandEntity();
        var toVo = new SandEntityToSandwich(_dt);

        // Convert Sandwich → SandEntity → Sandwich 500k times
        Sandwich result = sandwichVo;
        for (int i = 0; i < 500_000; i++)
        {
            var entity = toEntity.Translate(result);
            result = toVo.Translate(entity);
        }

        Assert.Equal(sandwichVo.SandId, result.SandId);
        Assert.Equal(sandwichVo.Custard.Phone1.Number, result.Custard.Phone1.Number);
        Assert.Equal(TimeSpan.Zero, result.Date.Offset);
        Assert.Equal(TimeSpan.Zero, result.DateCancelled.Offset);
    }
}
