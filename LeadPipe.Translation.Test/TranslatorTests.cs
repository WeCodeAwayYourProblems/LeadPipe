using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Translation.Primitives;
using LeadPipe.Translation.Translate.DtoToVo;
using LeadPipe.Translation.Translate.EntityToVo;

namespace LeadPipe.Translation.Test;

public class TranslatorTests
{
    private readonly DateTimeTranslate _dt = new();

    [Fact]
    public void CalliDtoToPlumbing_LocalTimes_ShouldConvertToUtcOffsetZero()
    {
        // Arrange
        var translator = new CalliDtoToPlumbing(_dt);
        var dto = new CalliDto
        {
            Phone = 5551234567,
            Date = "2025-03-09", // DST gap in many US zones
            Time = "02:30",       // Invalid local time
            TimeZone = "mst",
            PestProblem = "Test"
        };

        // Act
        var vo = translator.Translate(dto);

        // Assert
        Assert.Equal(TimeSpan.Zero, vo.Date.Offset);
        Assert.Equal(new PhoneNumber(dto.Phone), vo.PhoneNumber);
        Assert.Equal("Test", vo.Contents);
    }

    [Fact]
    public void CalliDtoToPlumbing_AmbiguousTime_ShouldConvertToUtcOffsetZero()
    {
        // Arrange
        var translator = new CalliDtoToPlumbing(_dt);
        var dto = new CalliDto
        {
            Phone = 5551234567,
            Date = "2025-11-02", // DST fallback
            Time = "01:30",
            TimeZone = "mst",
            PestProblem = "Fall-back test"
        };

        // Act
        var vo = translator.Translate(dto);

        // Assert
        Assert.Equal(TimeSpan.Zero, vo.Date.Offset);
        Assert.Equal("Fall-back test", vo.Contents);
    }

    [Theory]
    [InlineData("pst", "2025-06-01", "12:00")]  // Standard normal time
    [InlineData("mst", "2024-02-29", "00:00")]  // Leap year midnight
    [InlineData("cst", "2025-12-31", "23:59")]  // End of year
    [InlineData("est", "2025-03-10", "02:00")]  // Another DST gap
    public void CalliDtoToPlumbing_VariousLocalTimes_ShouldProduceUtc(string tzAbbr, string date, string time)
    {
        var translator = new CalliDtoToPlumbing(_dt);
        var dto = new CalliDto
        {
            Phone = 5551234567,
            Date = date,
            Time = time,
            TimeZone = tzAbbr,
            PestProblem = "Edge case test"
        };

        var vo = translator.Translate(dto);

        Assert.Equal(TimeSpan.Zero, vo.Date.Offset);
        Assert.Equal(new PhoneNumber(dto.Phone), vo.PhoneNumber);
        Assert.Equal("Edge case test", vo.Contents);
    }

    [Fact]
    public void LeafDtoToPlumbing_LocalTime_ShouldConvertToUtcOffsetZero()
    {
        var translator = new LeafDtoToPlumbing(_dt);
        var dto = new LeafDto
        {
            creation = new DateTime(2025, 3, 9, 2, 30, 0), // local DST gap
            prospect = new Prospect { cellphone = "5551234567" },
            messages =
            [
                new Message { creation = DateTime.MinValue, message = "Hello" }
            ]
        };

        var vo = translator.Translate(dto);

        Assert.Equal(TimeSpan.Zero, vo.Date.Offset);
        Assert.Equal("Hello", vo.Contents);
        Assert.Equal(new PhoneNumber("5551234567"), vo.PhoneNumber);
    }

    [Fact]
    public void SubsToSandwich_LocalTimes_ShouldProduceUtcOffsetZero()
    {
        var translator = new SubsToSandwich(_dt);
        var entity = new SubsEntity
        {
            Date = new DateTime(2025, 3, 9, 2, 0, 0),      // DST gap
            SubDate = new DateTime(2025, 6, 1, 12, 0, 0), // normal
            CancelDate = new DateTime(2025, 11, 2, 1, 30, 0), // ambiguous
            SubCancelDate = new DateTime(2025, 12, 31, 23, 59, 59),
            Number = 5555550001,
            Number2 = 5555550002,
            Active = true,
            SubActive = false,
            Complete = false,
            Value = 100,
            Seller = 1,
            Seller2 = 2,
            Seller3 = 3
        };

        var vo = translator.Translate(entity);

        Assert.Equal(TimeSpan.Zero, vo.Date.Offset);
        Assert.Equal(TimeSpan.Zero, vo.SubDate.Offset);
        Assert.Equal(TimeSpan.Zero, vo.CancelDate.Offset);
        Assert.Equal(TimeSpan.Zero, vo.SubCancelDate.Offset);

        Assert.Equal(5555550001, vo.Number.Number);
        Assert.Equal(5555550002, vo.Number2.Number);
    }

    [Fact]
    public void RoundTrip_LocalTime_CalliDtoToPlumbing_ShouldBeIdempotent()
    {
        var translator = new CalliDtoToPlumbing(_dt);
        var dto = new CalliDto
        {
            Phone = 5551234567,
            Date = "2025-03-09",
            Time = "02:30", // DST gap
            TimeZone = "mst",
            PestProblem = "Round-trip"
        };

        var vo1 = translator.Translate(dto);
        var vo2 = translator.Translate(dto);

        // Round-trip idempotency: repeated translations produce same UTC
        Assert.Equal(vo1.Date.UtcDateTime, vo2.Date.UtcDateTime);
        Assert.Equal(TimeSpan.Zero, vo2.Date.Offset);
    }
}
