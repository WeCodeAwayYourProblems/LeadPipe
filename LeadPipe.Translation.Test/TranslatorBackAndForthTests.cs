using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Translation.Primitives;
using LeadPipe.Translation.Translate.DtoToVo;

namespace LeadPipe.Translation.Test;

public class TranslatorBackAndForthTests
{
    private readonly DateTimeTranslate _dt = new();

    [Fact]
    public void CalliDtoToPlumbing_ThousandBackAndForthTranslations_ShouldRemainIdempotent()
    {
        // Arrange: create input DTO and expected VO output
        var dto = new CalliDto
        {
            Phone = "5551234567",
            Date = "2025-03-09", // DST gap
            Time = "02:30",
            TimeZone = "mst",
            PestProblem = "Test Problem"
        };
        var translator = new CalliDtoToPlumbing(_dt);

        // Expected VO
        DateTime localDateTime = DateTime.Parse(dto.Date + " " + dto.Time);
        DateTimeOffset expectedDate = _dt.Convert(localDateTime, ETimeZone.Mountain);
        var expectedVo = new Plumbing(
            Id: 0,
            PhoneNumber: new PhoneNumber(dto.Phone),
            Date: expectedDate,
            Contents: dto.PestProblem,
            Branch: "Elm",
            MetaData: "Test",
            Source: Source.Calli,
            Numbers: null
        );

        // Act: Perform 1000 back-and-forth translations
        Plumbing currentVo = expectedVo;
        for (int i = 0; i < 1000; i++)
        {
            // Convert VO back to DTO
            var roundDto = new CalliDto
            {
                Phone = currentVo.PhoneNumber.Number.ToString(),
                Date = currentVo.Date.UtcDateTime.ToString("yyyy-MM-dd"),
                Time = currentVo.Date.UtcDateTime.ToString("HH:mm"),
                TimeZone = "utc", // already UTC
                PestProblem = currentVo.Contents
            };

            // Convert DTO back to VO
            currentVo = translator.Translate(roundDto);
        }

        // Assert: VO after 1000 translations equals expected VO
        Assert.Equal(expectedVo.Date.UtcDateTime, currentVo.Date.UtcDateTime);
        Assert.Equal(TimeSpan.Zero, currentVo.Date.Offset);
        Assert.Equal(expectedVo.PhoneNumber, currentVo.PhoneNumber);
        Assert.Equal(expectedVo.Contents, currentVo.Contents);
        Assert.Equal(expectedVo.Source, currentVo.Source);
    }

    [Fact]
    public void LeafDtoToPlumbing_ThousandBackAndForthTranslations_ShouldRemainIdempotent()
    {
        // Arrange
        var dto = new LeafDto
        {
            creation = new DateTime(2025, 3, 9, 2, 30, 0), // DST gap
            prospect = new Prospect { cellphone = "5559876543" },
            messages =
            [
                new Message { creation = DateTime.MinValue, message = "Hello world" }
            ]
        };
        var translator = new LeafDtoToPlumbing();

        // Expected VO
        DateTimeOffset expectedDate = _dt.Convert(DateTime.SpecifyKind(dto.creation, DateTimeKind.Unspecified), ETimeZone.Mountain);
        var expectedVo = new Plumbing(
            Id: 0,
            PhoneNumber: new PhoneNumber(dto.prospect.cellphone),
            Date: expectedDate,
            Contents: dto.messages[0].message!,
            Branch: "Elm",
            MetaData: "Test",
            Source: Source.Leaf,
            Numbers: null
        );

        // Act: 1000 round-trip translations
        Plumbing currentVo = expectedVo;
        for (int i = 0; i < 1000; i++)
        {
            // VO -> DTO
            var roundDto = new LeafDto
            {
                creation = currentVo.Date.UtcDateTime,
                prospect = new Prospect { cellphone = $"{currentVo.PhoneNumber.Number}" },
                messages = [new Message { creation = DateTime.MinValue, message = currentVo.Contents }]
            };

            // DTO -> VO
            currentVo = translator.Translate(roundDto);
        }

        // Assert
        Assert.Equal(expectedVo.Date.UtcDateTime, currentVo.Date.UtcDateTime);
        Assert.Equal(TimeSpan.Zero, currentVo.Date.Offset);
        Assert.Equal(expectedVo.PhoneNumber, currentVo.PhoneNumber);
        Assert.Equal(expectedVo.Contents, currentVo.Contents);
        Assert.Equal(expectedVo.Source, currentVo.Source);
    }
}
