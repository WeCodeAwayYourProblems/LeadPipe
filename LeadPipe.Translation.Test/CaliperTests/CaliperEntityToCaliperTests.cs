using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Translation.Translate.EntityToVo;

namespace LeadPipe.Translation.Test.CaliperTests;

public sealed class CaliperEntityToCaliperTests
{
    private static CaliperEntity CreateEntity(
        DateTime date,
        int durationSeconds = 120,
        long phone = 5551234567,
        bool billable = true)
    {
        return new CaliperEntity
        {
            Id = 42,
            PhoneNumber = phone,
            Date = date,
            Duration = durationSeconds,
            Note = "Test note",
            Source = "Test source",
            Location = "Test location",
            Billable = billable
        };
    }

    [Fact]
    public void Translate_ShouldMapAllFieldsExactly()
    {
        // Arrange
        var inputDate = new DateTime(2025, 06, 01, 12, 0, 0, DateTimeKind.Unspecified);
        var entity = CreateEntity(inputDate);
        var translator = new CaliperEntityToCaliper();

        // Act
        Caliper result = translator.Translate(entity);

        // Assert
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.PhoneNumber, result.Number.Number);
        Assert.Equal(entity.Note, result.Note);
        Assert.Equal(entity.Source, result.Source);
        Assert.Equal(entity.Location, result.Location);
        Assert.Equal(entity.Billable, result.Billable);
        Assert.Equal(TimeSpan.FromSeconds(entity.Duration), result.Duration);
    }

    [Fact]
    public void Translate_ShouldForceUtcKindAndZeroOffset()
    {
        // Arrange — DST gap (spring forward)
        var localGap = new DateTime(2025, 3, 9, 2, 30, 0, DateTimeKind.Unspecified);
        var entity = CreateEntity(localGap);
        var translator = new CaliperEntityToCaliper();

        // Act
        Caliper result = translator.Translate(entity);

        // Assert
        Assert.Equal(DateTimeKind.Utc, result.Date.UtcDateTime.Kind);
        Assert.Equal(TimeSpan.Zero, result.Date.Offset);
        Assert.Equal(localGap, result.Date.UtcDateTime);
    }

    [Theory]
    [InlineData(2025, 11, 2, 1, 30)]  // DST fallback (ambiguous)
    [InlineData(2024, 2, 29, 0, 0)]   // Leap day
    [InlineData(2025, 12, 31, 23, 59)]// End of year
    public void Translate_ShouldPreserveExactInstant(
        int y, int m, int d, int h, int min)
    {
        // Arrange
        var dt = new DateTime(y, m, d, h, min, 0, DateTimeKind.Unspecified);
        var entity = CreateEntity(dt);
        var translator = new CaliperEntityToCaliper();

        // Act
        Caliper result = translator.Translate(entity);

        // Assert
        Assert.Equal(dt, result.Date.UtcDateTime);
        Assert.Equal(TimeSpan.Zero, result.Date.Offset);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1_000)]
    [InlineData(100_000)]
    public void Translate_Repeatedly_ShouldBeIdempotent(int iterations)
    {
        // Arrange
        var dt = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Unspecified);
        var entity = CreateEntity(dt);
        var translator = new CaliperEntityToCaliper();

        Caliper current = translator.Translate(entity);

        // Act
        for (int i = 0; i < iterations; i++)
        {
            current = translator.Translate(entity);
        }

        // Assert
        Assert.Equal(entity.Id, current.Id);
        Assert.Equal(entity.PhoneNumber, current.Number.Number);
        Assert.Equal(dt, current.Date.UtcDateTime);
        Assert.Equal(TimeSpan.Zero, current.Date.Offset);
        Assert.Equal(TimeSpan.FromSeconds(entity.Duration), current.Duration);
    }
}
