using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Translation.Primitives;
using LeadPipe.Translation.Translate.EntityToVo;

namespace LeadPipe.Translation.Test.Sand;

public sealed class SandMySqlEntityToSandwichTests
{
    private readonly IDateTimeTranslate _dt = Substitute.For<IDateTimeTranslate>();

    private SandMySqlEntity CreateEntity()
    {
        var cust = new CustardMySqlEntity
        {
            customerID = 10,
            status = 1,
            phone1 = "5551002000",
            phone2 = "5551003000",
            dateAdded = new DateTime(2025, 6, 1, 12, 0, 0),
            dateCancelled = new DateTime(2025, 12, 31, 23, 59, 59)
        };

        var entity = new SandMySqlEntity
        {
            subscriptionID = 99,
            customerID = 10,
            customer = cust,
            dateAdded = new DateTime(2025, 6, 1, 12, 0, 0),
            dateCancelled = new DateTime(2025, 12, 31, 23, 59, 59),
            active = 1,
            initialStatus = 1,
            serviceType = "Premium",
            contractValue = 199.99m,
            soldBy = 2,
            soldBy2 = 3,
            soldBy3 = 4
        };

        return entity;
    }

    [Fact]
    public void Translate_ShouldThrow_WhenCustomerIsNull()
    {
        var entity = CreateEntity();
        entity.customer = null;

        var translator = new SandMySqlEntityToSandwich(_dt);

        Assert.Throws<ArgumentException>(() => translator.Translate(entity));
    }

    [Fact]
    public void Translate_ShouldMapAllFields()
    {
        var entity = CreateEntity();
        _dt.Convert(entity.dateAdded, ETimeZone.Pacific).Returns(new DateTimeOffset(entity.dateAdded, TimeSpan.Zero));
        _dt.Convert(entity.dateCancelled, ETimeZone.Pacific).Returns(new DateTimeOffset(entity.dateCancelled, TimeSpan.Zero));
        _dt.Convert(entity.customer.dateAdded, ETimeZone.Pacific).Returns(new DateTimeOffset(entity.customer.dateAdded, TimeSpan.Zero));
        _dt.Convert(entity.customer.dateCancelled, ETimeZone.Pacific).Returns(new DateTimeOffset(entity.customer.dateCancelled, TimeSpan.Zero));

        var translator = new SandMySqlEntityToSandwich(_dt);

        Sandwich vo = translator.Translate(entity);

        Assert.Equal(99, vo.SandId);
        Assert.Equal(10, vo.CustardId);
        Assert.Equal("Premium", vo.Type);
        Assert.Equal(199.99m, vo.Value);
        Assert.Equal(true, vo.Active);
        Assert.Equal(true, vo.Complete);
        Assert.Equal(2, vo.Seller);
        Assert.Equal(3, vo.Seller2);
        Assert.Equal(4, vo.Seller3);

        Assert.Equal(10, vo.Custard.Id);
        Assert.Equal(true, vo.Custard.Status);
        Assert.Equal(5551002000, vo.Custard.Phone1.Number);
        Assert.Equal(5551003000, vo.Custard.Phone2.Number);

        // Dates remain UTC
        Assert.Equal(TimeSpan.Zero, vo.Date.Offset);
        Assert.Equal(TimeSpan.Zero, vo.DateCancelled.Offset);
    }

    [Fact]
    public void Translate_ShouldHandleDSTAndEdgeDates()
    {
        var entity = CreateEntity();
        _dt.Convert(Arg.Any<DateTime>(), Arg.Any<ETimeZone>())
            .Returns(callInfo => new DateTimeOffset((DateTime)callInfo[0], TimeSpan.Zero));

        var translator = new SandMySqlEntityToSandwich(_dt);

        Sandwich vo = translator.Translate(entity);

        // UTC offset
        Assert.Equal(TimeSpan.Zero, vo.Date.Offset);
        Assert.Equal(TimeSpan.Zero, vo.DateCancelled.Offset);
        Assert.Equal(TimeSpan.Zero, vo.Custard.Date.Offset);
        Assert.Equal(TimeSpan.Zero, vo.Custard.DateCancelled.Offset);
    }

    [Fact]
    public void Translate_ShouldRemainIdempotent_UnderLargeScale()
    {
        var entity = CreateEntity();
        _dt.Convert(Arg.Any<DateTime>(), Arg.Any<ETimeZone>()).Returns(callInfo => new DateTimeOffset((DateTime)callInfo[0], TimeSpan.Zero));

        var translator = new SandMySqlEntityToSandwich(_dt);

        Sandwich current = translator.Translate(entity);
        for (int i = 0; i < 1_000_000; i++)
        {
            current = translator.Translate(entity);
        }

        Assert.Equal(99, current.SandId);
        Assert.Equal(10, current.CustardId);
        Assert.Equal(5551002000, current.Custard.Phone1.Number);
        Assert.Equal(5551003000, current.Custard.Phone2.Number);
    }
}
