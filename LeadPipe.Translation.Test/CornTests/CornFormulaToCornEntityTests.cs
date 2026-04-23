using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Translation.Translate.VoToEntity;

namespace LeadPipe.Translation.Test.Corn;

public sealed class CornFormulaToCornEntityTests
{
    private static CornFormula CreateVo(DateTimeOffset date)
        => new(
            Id: 77,
            PhoneNumber: new PhoneNumber(5558887777),
            Date: date,
            PayLoad: "payload",
            MetaData: "meta",
            Source: "corn"
        );

    [Fact]
    public void Translate_ShouldMapUtcDateCorrectly()
    {
        var date = new DateTimeOffset(2024, 2, 29, 23, 59, 59, TimeSpan.Zero);
        var vo = CreateVo(date);
        var translator = new CornFormulaToCornEntity();

        var entity = translator.Translate(vo);

        Assert.Equal(date.UtcDateTime, entity.Date);
        Assert.Equal(date.ToUnixTimeSeconds(), entity.UnixDate);
        Assert.Equal(DateTimeKind.Utc, entity.Date.Kind);
    }

    [Fact]
    public void Translate_ShouldMapAllFields()
    {
        var vo = CreateVo(DateTimeOffset.UtcNow);
        var translator = new CornFormulaToCornEntity();

        var entity = translator.Translate(vo);

        Assert.Equal(vo.Id, entity.Id);
        Assert.Equal(5558887777, entity.PhoneNumber.Number);
        Assert.Equal(vo.PayLoad, entity.Payload);
        Assert.Equal(vo.MetaData, entity.MetaData);
        Assert.Equal(vo.Source, entity.Source);
    }

    [Fact]
    public void Translate_ShouldRemainIdempotent_UnderHeavyRepetition()
    {
        var vo = CreateVo(
            new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero)
        );
        var translator = new CornFormulaToCornEntity();

        CornEntity current = translator.Translate(vo);

        for (int i = 0; i < 500_000; i++)
        {
            current = translator.Translate(vo);
        }

        Assert.Equal(vo.Date.UtcDateTime, current.Date);
        Assert.Equal(vo.Date.ToUnixTimeSeconds(), current.UnixDate);
        Assert.Equal(5558887777, current.PhoneNumber.Number);
    }
}
