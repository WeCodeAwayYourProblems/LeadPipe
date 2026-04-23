using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Translation.Translate.VoToEntity;

namespace LeadPipe.Translation.Test.CustardTests;

public sealed class CustardToCustardEntityTests
{
    private static Custard CreateVo(DateTimeOffset date, DateTimeOffset cancel)
        => new(
            Id: 99,
            Status: true,
            Phone1: new PhoneNumber(5551112222),
            Phone2: new PhoneNumber(5553334444),
            Date: date,
            DateCancelled: cancel
        );

    [Fact]
    public void Translate_ShouldMapUtcDatesAndUnixCorrectly()
    {
        var date = new DateTimeOffset(2025, 6, 1, 12, 0, 0, TimeSpan.Zero);
        var cancel = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero);
        var vo = CreateVo(date, cancel);
        var translator = new CustardToCustardEntity();

        CustardEntity entity = translator.Translate(vo);

        Assert.Equal(date.UtcDateTime, entity.Date);
        Assert.Equal(cancel.UtcDateTime, entity.CancelDate);
        Assert.Equal(date.ToUnixTimeSeconds(), entity.UnixDate);
        Assert.Equal(cancel.ToUnixTimeSeconds(), entity.UnixCancelDate);
        Assert.Equal(DateTimeKind.Utc, entity.Date.Kind);
    }

    [Fact]
    public void Translate_ShouldMapAllFields()
    {
        var vo = CreateVo(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        var translator = new CustardToCustardEntity();

        CustardEntity entity = translator.Translate(vo);

        Assert.Equal(99, entity.Id);
        Assert.True(entity.Active);
        Assert.Equal(5551112222, entity.PhoneNumber.Number);
        Assert.Equal(5553334444, entity.PhoneNumber2?.Number);
    }

    [Fact]
    public void Translate_ShouldRemainIdempotent_UnderHeavyRepetition()
    {
        var vo = CreateVo(
            new DateTimeOffset(2024, 2, 29, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 3, 9, 2, 30, 0, TimeSpan.Zero)
        );
        var translator = new CustardToCustardEntity();

        CustardEntity current = translator.Translate(vo);

        for (int i = 0; i < 500_000; i++)
        {
            current = translator.Translate(vo);
        }

        Assert.Equal(vo.Date.UtcDateTime, current.Date);
        Assert.Equal(vo.Date.ToUnixTimeSeconds(), current.UnixDate);
        Assert.Equal(5551112222, current.PhoneNumber.Number);
    }
}
