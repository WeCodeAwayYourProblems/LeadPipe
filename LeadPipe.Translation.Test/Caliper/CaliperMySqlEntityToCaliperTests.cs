using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Translation.Translate.EntityToVo;

namespace LeadPipe.Translation.Test.Caliper;

public sealed class CaliperMySqlEntityToCaliperTests
{
    private static CaliperMySqlEntity CreateEntity(
        DateTime calledAtUtc,
        int? duration = null,
        string? phone = "5551234567",
        string? billable = "billable",
        IEnumerable<(string? transcription, string? summary)>? text = null)
    {
        var entity = new CaliperMySqlEntity
        {
            call_id = 99,
            called_at_utc = calledAtUtc,
            duration = duration,
            contact_number_clean = phone,
            sale_billable = billable,
            source = "mysql",
            location = "ivr",
            transcriptions = [],
            summaries = []
        };

        if (text is not null)
        {
            foreach (var (t, s) in text)
            {
                if (t is not null)
                    entity.transcriptions.Add(new TranscriptionMySqlEntity { transcription = t });

                if (s is not null)
                    entity.summaries.Add(new SummaryMySqlEntity { summary = s });
            }
        }

        return entity;
    }

    [Fact]
    public void Translate_ShouldMapAllSimpleFields()
    {
        // Arrange
        var dt = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var entity = CreateEntity(dt, duration: 90);
        var translator = new CaliperMySqlEntityToCaliper();

        // Act
        var result = translator.Translate(entity);

        // Assert
        Assert.Equal(entity.call_id, result.Id);
        Assert.Equal(5551234567, result.Number.Number);
        Assert.Equal(TimeSpan.FromSeconds(90), result.Duration);
        Assert.Equal("mysql", result.Source);
        Assert.Equal("ivr", result.Location);
        Assert.True(result.Billable);
    }

    [Fact]
    public void Translate_ShouldForceUtcOffsetZero()
    {
        // Arrange
        var dt = new DateTime(2025, 3, 9, 2, 30, 0, DateTimeKind.Unspecified); // DST gap
        var entity = CreateEntity(dt);
        var translator = new CaliperMySqlEntityToCaliper();

        // Act
        var result = translator.Translate(entity);

        // Assert
        Assert.Equal(dt, result.Date.UtcDateTime);
        Assert.Equal(TimeSpan.Zero, result.Date.Offset);
        Assert.Equal(DateTimeKind.Utc, result.Date.UtcDateTime.Kind);
    }

    [Fact]
    public void Translate_InvalidPhone_ShouldFallbackToDefault()
    {
        // Arrange
        var entity = CreateEntity(DateTime.UtcNow, phone: "invalid");
        var translator = new CaliperMySqlEntityToCaliper();

        // Act
        var result = translator.Translate(entity);

        // Assert
        Assert.Equal(PhoneNumber.Default, result.Number.Number);
    }

    [Fact]
    public void Translate_NullOrNonIntDuration_ShouldBeZero()
    {
        var translator = new CaliperMySqlEntityToCaliper();

        var nullDuration = CreateEntity(DateTime.UtcNow, duration: null);
        var stringDuration = CreateEntity(DateTime.UtcNow, duration: -1);

        Assert.Equal(TimeSpan.Zero, translator.Translate(nullDuration).Duration);
        Assert.Equal(TimeSpan.Zero, translator.Translate(stringDuration).Duration);
    }

    [Fact]
    public void Translate_TranscriptionsAndSummaries_ShouldConcatenateInOrder()
    {
        // Arrange
        var entity = CreateEntity(
            DateTime.UtcNow,
            duration: 30,
            text:
            [
                ("Hello", null),
            (null, "Summary"),
            ("World", "Second")
            ]);

        var translator = new CaliperMySqlEntityToCaliper();

        // Act
        var result = translator.Translate(entity);

        // Assert
        Assert.Contains("Summary", result.Note);
        Assert.Contains("Second", result.Note);
        Assert.Contains("Hello", result.Note);
        Assert.Contains("World", result.Note);
    }

    [Theory]
    [InlineData("billable", true)]
    [InlineData("non billable", false)]
    [InlineData(null, false)]
    public void Translate_BillableFlag_ShouldBeDerivedCorrectly(string? value, bool expected)
    {
        // Arrange
        var entity = CreateEntity(DateTime.UtcNow, billable: value);
        var translator = new CaliperMySqlEntityToCaliper();

        // Act
        var result = translator.Translate(entity);

        // Assert
        Assert.Equal(expected, result.Billable);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1_000)]
    [InlineData(100_000)]
    public void Translate_Repeatedly_ShouldRemainIdempotent(int iterations)
    {
        // Arrange
        var dt = new DateTime(2024, 2, 29, 0, 0, 0, DateTimeKind.Utc);
        var entity = CreateEntity(dt, duration: 45);
        var translator = new CaliperMySqlEntityToCaliper();

        Caliper current = translator.Translate(entity);

        // Act
        for (int i = 0; i < iterations; i++)
        {
            current = translator.Translate(entity);
        }

        // Assert
        Assert.Equal(entity.call_id, current.Id);
        Assert.Equal(dt, current.Date.UtcDateTime);
        Assert.Equal(TimeSpan.Zero, current.Date.Offset);
        Assert.Equal(TimeSpan.FromSeconds(45), current.Duration);
    }
}
