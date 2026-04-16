using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Translation.Primitives;
using LeadPipe.Translation.Translate.EntityToVo;
using NSubstitute;

namespace LeadPipe.Translation.Test.CustardTests;

public sealed class CustardMySqlEntityToCustardTests
{
    private readonly IDateTimeTranslate dt = Substitute.For<IDateTimeTranslate>();
    private static CustardMySqlEntity CreateEntity(
        int status = 1,
        string? phone1 = "5551112222",
        string? phone2 = "5553334444")
        => new()
        {
            customerID = 99,
            status = status,
            phone1 = phone1,
            phone2 = phone2,
            dateAdded = new DateTime(2025, 6, 1, 12, 0, 0),
            dateCancelled = new DateTime(2025, 12, 31, 23, 59, 59)
        };

    [Fact]
    public void Translate_ShouldMapAllFieldsCorrectly()
    {
        var entity = CreateEntity();
        var translator = new CustardMySqlEntityToCustard(dt);

        Custard vo = translator.Translate(entity);

        Assert.Equal(99, vo.Id);
        Assert.True(vo.Status);
        Assert.Equal(5551112222, vo.Phone1.Number);
        Assert.Equal(5553334444, vo.Phone2?.Number);
        Assert.Equal(entity.dateAdded, vo.Date.UtcDateTime);
        Assert.Equal(entity.dateCancelled, vo.DateCancelled?.UtcDateTime);
        Assert.Equal(TimeSpan.Zero, vo.Date.Offset);
        Assert.Equal(TimeSpan.Zero, vo.DateCancelled?.Offset);
    }

    [Fact]
    public void Translate_StatusZero_ShouldProduceFalse()
    {
        var entity = CreateEntity(status: 0);
        var translator = new CustardMySqlEntityToCustard(dt);

        Custard vo = translator.Translate(entity);

        Assert.False(vo.Status);
    }

    [Fact]
    public void Translate_InvalidPhones_ShouldFallbackToDefault()
    {
        var entity = CreateEntity(phone1: "xxx", phone2: null);
        var translator = new CustardMySqlEntityToCustard(dt);

        Custard vo = translator.Translate(entity);

        Assert.Equal(PhoneNumber.Default, vo.Phone1.Number);
        Assert.Equal(PhoneNumber.Default, vo.Phone2?.Number);
    }

    [Theory]
    [InlineData(2024, 2, 29)]
    [InlineData(2025, 3, 9)]   // DST gap
    [InlineData(2025, 11, 2)]  // DST fallback
    public void Translate_ShouldPreserveExactDate(int y, int m, int d)
    {
        var entity = CreateEntity();
        entity.dateAdded = new DateTime(y, m, d, 1, 30, 0);
        var translator = new CustardMySqlEntityToCustard(dt);

        Custard vo = translator.Translate(entity);

        Assert.Equal(entity.dateAdded, vo.Date.UtcDateTime);
        Assert.Equal(TimeSpan.Zero, vo.Date.Offset);
    }

    [Fact]
    public void Translate_ShouldRemainIdempotent_AtScale()
    {
        var entity = CreateEntity();
        var translator = new CustardMySqlEntityToCustard(dt);

        Custard current = translator.Translate(entity);

        for (int i = 0; i < 1_000_000; i++)
        {
            current = translator.Translate(entity);
        }

        Assert.Equal(99, current.Id);
        Assert.True(current.Status);
        Assert.Equal(5551112222, current.Phone1.Number);
        Assert.Equal(TimeSpan.Zero, current.Date.Offset);
    }
}
