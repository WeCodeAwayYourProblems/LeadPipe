using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Translation.Translate.EntityToVo;

namespace LeadPipe.Translation.Test.PlumbingTests;

public sealed class PlumbingEntityToPlumbingTests
{
    private static PlumbingEntity CreateEntity(DateTime date)
        => new()
        {
            Id = 42,
            PhoneNumber = new(5558889999),
            Date = date,
            UnixDate = new DateTimeOffset(date, TimeSpan.Zero).ToUnixTimeSeconds(),
            Contents = "Pest issue",
            MetaData = "Meta",
            Source = Source.Leaf
        };

    [Fact]
    public void Translate_ShouldProduceUtcOffsetZero()
    {
        var date = new DateTime(2025, 3, 9, 2, 30, 0); // DST gap
        var entity = CreateEntity(date);
        var translator = new PlumbingEntityToPlumbing();

        Plumbing vo = translator.Translate(entity);

        Assert.Equal(TimeSpan.Zero, vo.Date.Offset);
        Assert.Equal(DateTime.SpecifyKind(date, DateTimeKind.Utc), vo.Date.UtcDateTime);
    }

    [Fact]
    public void Translate_ShouldMapAllFields()
    {
        var date = new DateTime(2025, 6, 1, 12, 0, 0);
        var entity = CreateEntity(date);
        var translator = new PlumbingEntityToPlumbing();

        Plumbing vo = translator.Translate(entity);

        Assert.Equal(42, vo.Id);
        Assert.Equal(5558889999, vo.PhoneNumber.Number);
        Assert.Equal("Pest issue", vo.Contents);
        Assert.Equal("Meta", vo.MetaData);
        Assert.Equal(Source.Leaf, vo.Source);
    }

    [Theory]
    [InlineData(2024, 2, 29)]
    [InlineData(2025, 11, 2)]
    [InlineData(2025, 12, 31)]
    public void Translate_EdgeDates_ShouldRemainStable(int y, int m, int d)
    {
        var date = new DateTime(y, m, d, 1, 30, 0);
        var entity = CreateEntity(date);
        var translator = new PlumbingEntityToPlumbing();

        Plumbing vo = translator.Translate(entity);

        Assert.Equal(DateTime.SpecifyKind(date, DateTimeKind.Utc), vo.Date.UtcDateTime);
        Assert.Equal(TimeSpan.Zero, vo.Date.Offset);
    }

    [Fact]
    public void Translate_ShouldRemainIdempotent_AtScale()
    {
        var date = new DateTime(2025, 6, 1, 12, 0, 0);
        var entity = CreateEntity(date);
        var translator = new PlumbingEntityToPlumbing();

        Plumbing current = translator.Translate(entity);

        for (int i = 0; i < 1_000_000; i++)
        {
            current = translator.Translate(entity);
        }

        Assert.Equal(5558889999, current.PhoneNumber.Number);
        Assert.Equal(TimeSpan.Zero, current.Date.Offset);
        Assert.Equal(entity.Date, current.Date.UtcDateTime);
    }
}
