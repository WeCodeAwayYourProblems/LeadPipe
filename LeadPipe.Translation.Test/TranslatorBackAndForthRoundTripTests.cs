using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;
using LeadPipe.Translation.Primitives;
using LeadPipe.Translation.Translate.EntityToVo;
using LeadPipe.Translation.Translate.VoToEntity;

namespace LeadPipe.Translation.Test;

public class TranslatorBackAndForthRoundTripTests
{
    private readonly IDateTimeTranslate _dt = Substitute.For<IDateTimeTranslate>();

    // Utility for repeated round-trip
    private TVo RoundTrip<TEntity, TVo>(
        TEntity entity,
        IEntityToVo<TEntity, TVo> toVo,
        IVoToEntity<TVo, TEntity> toEntity,
        int iterations = 1_000
    )
    {
        TVo vo = toVo.Translate(entity);
        for (int i = 0; i < iterations; i++)
        {
            entity = toEntity.Translate(vo);
            vo = toVo.Translate(entity);
        }
        return vo;
    }

    [Fact]
    public void CaliperEntity_To_Caliper_RoundTrip_ShouldRemainConsistent()
    {
        var entity = new CaliperEntity
        {
            Id = 1,
            PhoneNumber = 5551234567,
            CaliperDate = new DateTime(2025, 6, 1, 12, 0, 0),
            Duration = 123,
            Note = "Note",
            Source = "Source",
            Location = "Location",
            Billable = true
        };

        var toVo = new CaliperEntityToCaliper();
        var toEntity = new CaliperToCaliperEntity();

        Caliper vo = RoundTrip(entity, toVo, toEntity, 1_000_000);

        Assert.Equal(entity.Id, vo.Id);
        Assert.Equal(entity.PhoneNumber, vo.Number.Number);
        Assert.Equal(TimeSpan.FromSeconds(entity.Duration), vo.Duration);
        Assert.Equal(entity.Note, vo.Note);
        Assert.Equal(TimeSpan.Zero, vo.Date.Offset);
    }

    [Fact]
    public void CaliperMySqlEntity_To_Caliper_RoundTrip_ShouldRemainConsistent()
    {
        var entity = new CaliperMySqlEntity
        {
            call_id = 1,
            contact_number_clean = "5551234567",
            called_at_utc = new DateTime(2025, 6, 1, 12, 0, 0),
            duration = 123,
            sale_billable = "billable",
            source = "Source",
            location = "Location",
            transcriptions = new List<Transcription> { new() { transcription = "Hello" } },
            summaries = new List<Summary> { new() { summary = "Summary" } }
        };

        var toVo = new CaliperMySqlEntityToCaliper();
        var toEntity = new CaliperToCaliperMySqlEntity();

        Caliper vo = RoundTrip(entity, toVo, toEntity, 1_000_000);

        Assert.Equal(entity.call_id, vo.Id);
        Assert.Equal(5551234567, vo.Number.Number);
        Assert.Equal(TimeSpan.FromSeconds(entity.duration), vo.Duration);
        Assert.Equal("Summary | Hello", vo.Note);
        Assert.Equal(TimeSpan.Zero, vo.Date.Offset);
    }

    [Fact]
    public void CornMySqlEntity_To_CornFormula_RoundTrip_ShouldRemainConsistent()
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

        var toVo = new CornMySqlEntityToCornFormula();
        var toEntity = new CornFormulaToCornEntity();

        CornFormula vo = RoundTrip(entity, toVo, toEntity, 500_000);

        Assert.Equal(entity.id, vo.Id);
        Assert.Equal(5551002000, vo.PhoneNumber.Number);
        Assert.Equal("Payload", vo.PayLoad);
        Assert.Equal("Form: FormX | Referring: http://ref.url", vo.MetaData);
        Assert.Equal(TimeSpan.Zero, vo.Date.Offset);
    }

    [Fact]
    public void CustardMySqlEntity_To_Custard_RoundTrip_ShouldRemainConsistent()
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

        var toVo = new CustardMySqlEntityToCustard();
        var toEntity = new CustardToCustardEntity();

        Custard vo = RoundTrip(entity, toVo, toEntity, 500_000);

        Assert.Equal(entity.customerID, vo.Id);
        Assert.Equal(true, vo.Status);
        Assert.Equal(5551002000, vo.Phone1.Number);
        Assert.Equal(5551003000, vo.Phone2.Number);
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
            Seller3: 4
        );

        var toEntity = new SandwichToSandEntity();
        var toVo = new SandToSandwich(_dt);

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
