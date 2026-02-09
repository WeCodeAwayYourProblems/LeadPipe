using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Translation.Translate.VoToEntity;

namespace LeadPipe.Translation.Test.CaliperTests;

public sealed class CaliperToCaliperEntityTests
{
    private static Caliper CreateVo(DateTimeOffset date, int durationSeconds = 61)
        => new(
            Id: 42,
            Date: date,
            Number: new PhoneNumber(5551234567),
            Duration: TimeSpan.FromSeconds(durationSeconds),
            Note: "test note",
            Source: "unit",
            Location: "lab",
            Billable: true
        );

    [Fact]
    public void Translate_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var date = new DateTimeOffset(2025, 6, 1, 12, 0, 0, TimeSpan.Zero);
        var vo = CreateVo(date);
        var translator = new CaliperToCaliperEntity();

        // Act
        var entity = translator.Translate(vo);

        // Assert
        Assert.Equal(vo.Id, entity.Id);
        Assert.Equal(5551234567, entity.PhoneNumber.Number);
        Assert.Equal(date.UtcDateTime, entity.Date);
        Assert.Equal(date.ToUnixTimeSeconds(), entity.UnixDate);
        Assert.Equal(vo.Note, entity.Note);
        Assert.Equal(vo.Source, entity.Source);
        Assert.Equal(vo.Location, entity.Location);
        Assert.Equal(61, entity.Duration);
        Assert.True(entity.Billable);
    }

    [Theory]
    [InlineData(61.9, 61)]
    [InlineData(0.9, 0)]
    [InlineData(120.0, 120)]
    public void Translate_Duration_ShouldTruncateSeconds(double seconds, int expected)
    {
        var vo = CreateVo(
            DateTimeOffset.UtcNow,
            durationSeconds: (int)seconds
        );

        var translator = new CaliperToCaliperEntity();
        var entity = translator.Translate(vo);

        Assert.Equal(expected, entity.Duration);
    }

    [Fact]
    public void Translate_ShouldRemainIdempotent_UnderHeavyRepetition()
    {
        // Arrange
        var date = new DateTimeOffset(2024, 2, 29, 0, 0, 0, TimeSpan.Zero);
        var vo = CreateVo(date, durationSeconds: 90);
        var translator = new CaliperToCaliperEntity();

        CaliperEntity current = translator.Translate(vo);

        // Act
        for (int i = 0; i < 500_000; i++)
        {
            current = translator.Translate(vo);
        }

        // Assert
        Assert.Equal(vo.Id, current.Id);
        Assert.Equal(date.UtcDateTime, current.Date);
        Assert.Equal(date.ToUnixTimeSeconds(), current.UnixDate);
        Assert.Equal(90, current.Duration);
    }
}
