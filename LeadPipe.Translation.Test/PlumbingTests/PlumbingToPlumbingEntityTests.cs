using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Translation.Translate.VoToEntity;

namespace LeadPipe.Translation.Test.PlumbingTests;

public sealed class PlumbingToPlumbingEntityTests
{
    private static Plumbing CreateVo(DateTimeOffset date)
        => new(
            Id: 42,
            PhoneNumber: new PhoneNumber(5558889999),
            Date: date,
            Contents: "Rodents",
            MetaData: "Meta",
            Branch: "Elm",
            Source: Source.Calli,
            Numbers: null
        );

    [Fact]
    public void Translate_ShouldConvertUtcCorrectly()
    {
        var date = new DateTimeOffset(2025, 6, 1, 12, 0, 0, TimeSpan.Zero);
        var vo = CreateVo(date);
        var translator = new PlumbingToPlumbingEntity();

        PlumbingEntity entity = translator.Translate(vo);

        Assert.Equal(date.UtcDateTime, entity.Date);
        Assert.Equal(date.ToUnixTimeSeconds(), entity.UnixDate);
        Assert.Equal(DateTimeKind.Utc, entity.Date.Kind);
    }

    [Fact]
    public void Translate_ShouldMapAllFields()
    {
        var vo = CreateVo(DateTimeOffset.UtcNow);
        var translator = new PlumbingToPlumbingEntity();

        PlumbingEntity entity = translator.Translate(vo);

        Assert.Equal(42, entity.Id);
        Assert.Equal(5558889999, entity.PhoneNumber.Number);
        Assert.Equal("Rodents", entity.Contents);
        Assert.Equal("Meta", entity.MetaData);
        Assert.Equal(Source.Calli, entity.Source);
    }

    [Fact]
    public void Translate_ShouldAllowNullContents()
    {
        var vo = new Plumbing(
            Id: 1,
            PhoneNumber: new PhoneNumber(5551110000),
            Date: DateTimeOffset.UtcNow,
            Contents: null,
            MetaData: "Meta",
            Branch: "Elm",
            Source: Source.Leaf,
            Numbers: null
        );
        var translator = new PlumbingToPlumbingEntity();

        PlumbingEntity entity = translator.Translate(vo);

        Assert.Null(entity.Contents);
    }

    [Fact]
    public void Translate_ShouldRemainIdempotent_UnderHeavyLoad()
    {
        var vo = CreateVo(
            new DateTimeOffset(2024, 2, 29, 0, 0, 0, TimeSpan.Zero)
        );
        var translator = new PlumbingToPlumbingEntity();

        PlumbingEntity current = translator.Translate(vo);

        for (int i = 0; i < 750_000; i++)
        {
            current = translator.Translate(vo);
        }

        Assert.Equal(vo.Date.UtcDateTime, current.Date);
        Assert.Equal(vo.Date.ToUnixTimeSeconds(), current.UnixDate);
    }
}
