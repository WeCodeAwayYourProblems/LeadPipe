using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Translation.Primitives;
using LeadPipe.Translation.Translate.EntityToVo;

namespace LeadPipe.Translation.Test.Sand;

public sealed class SandwichToSandEntityTests
{
    private readonly IDateTimeTranslate _dt = Substitute.For<IDateTimeTranslate>();

    private Sandwich CreateVo()
    {
        var cust = new Custard(
            Id: 10,
            Status: true,
            Phone1: new PhoneNumber(5551002000),
            Phone2: new PhoneNumber(5551003000),
            Date: new DateTimeOffset(2025, 6, 1, 12, 0, 0, TimeSpan.Zero),
            DateCancelled: new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero)
        );

        return new Sandwich(
            SandId: 99,
            CustardId: 10,
            Custard: cust,
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
    }

    [Fact]
    public void Translate_ShouldThrow_WhenCustardEntityNull()
    {
        var vo = CreateVo();
        var entity = new SandEntity
        {
            Id = vo.SandId,
            CustardId = vo.CustardId,
            CustardEntity = null
        };

        var translator = new SandToSandwich(_dt);

        Assert.Throws<ArgumentException>(() => translator.Translate(entity));
    }

    [Fact]
    public void Translate_ShouldMapAllFieldsCorrectly()
    {
        var vo = CreateVo();
        var entity = new SandEntity
        {
            Id = vo.SandId,
            CustardId = vo.CustardId,
            Date = vo.Date.UtcDateTime,
            CancelDate = vo.DateCancelled.UtcDateTime,
            Active = vo.Active,
            Complete = vo.Complete,
            Type = vo.Type,
            Value = vo.Value,
            Seller = vo.Seller,
            Seller2 = vo.Seller2,
            Seller3 = vo.Seller3,
            CustardEntity = new CustardEntity
            {
                Id = vo.Custard.Id,
                Active = vo.Custard.Status,
                PhoneNumber = vo.Custard.Phone1.Number,
                PhoneNumber2 = vo.Custard.Phone2.Number,
                Date = vo.Custard.Date.UtcDateTime,
                CancelDate = vo.Custard.DateCancelled.UtcDateTime
            }
        };

        _dt.Convert(Arg.Any<DateTime>(), Arg.Any<ETimeZone>())
            .Returns(callInfo => new DateTimeOffset((DateTime)callInfo[0], TimeSpan.Zero));

        var translator = new SandToSandwich(_dt);
        Sandwich result = translator.Translate(entity);

        Assert.Equal(vo.SandId, result.SandId);
        Assert.Equal(vo.CustardId, result.CustardId);
        Assert.Equal(vo.Type, result.Type);
        Assert.Equal(vo.Value, result.Value);
        Assert.Equal(vo.Custard.Phone1.Number, result.Custard.Phone1.Number);
        Assert.Equal(TimeSpan.Zero, result.Date.Offset);
        Assert.Equal(TimeSpan.Zero, result.DateCancelled.Offset);
    }

    [Fact]
    public void Translate_ShouldRemainIdempotent_AtScale()
    {
        var vo = CreateVo();
        var entity = new SandEntity
        {
            Id = vo.SandId,
            CustardId = vo.CustardId,
            Date = vo.Date.UtcDateTime,
            CancelDate = vo.DateCancelled.UtcDateTime,
            Active = vo.Active,
            Complete = vo.Complete,
            Type = vo.Type,
            Value = vo.Value,
            Seller = vo.Seller,
            Seller2 = vo.Seller2,
            Seller3 = vo.Seller3,
            CustardEntity = new CustardEntity
            {
                Id = vo.Custard.Id,
                Active = vo.Custard.Status,
                PhoneNumber = vo.Custard.Phone1.Number,
                PhoneNumber2 = vo.Custard.Phone2.Number,
                Date = vo.Custard.Date.UtcDateTime,
                CancelDate = vo.Custard.DateCancelled.UtcDateTime
            }
        };

        _dt.Convert(Arg.Any<DateTime>(), Arg.Any<ETimeZone>())
            .Returns(callInfo => new DateTimeOffset((DateTime)callInfo[0], TimeSpan.Zero));

        var translator = new SandToSandwich(_dt);

        Sandwich current = translator.Translate(entity);

        for (int i = 0; i < 500_000; i++)
        {
            current = translator.Translate(entity);
        }

        Assert.Equal(vo.SandId, current.SandId);
        Assert.Equal(vo.CustardId, current.CustardId);
        Assert.Equal(5551002000, current.Custard.Phone1.Number);
        Assert.Equal(5551003000, current.Custard.Phone2.Number);
        Assert.Equal(TimeSpan.Zero, current.Date.Offset);
        Assert.Equal(TimeSpan.Zero, current.DateCancelled.Offset);
    }
}
