using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Translation.Translate.DtoToVo;

namespace LeadPipe.Translation.Test.PlumbingTests;

public class LabDtoToPlumbingTests
{
    private readonly LabDtoToPlumbing _translator = new();

    [Fact]
    public void Translate_ValidDto_MapsAllFieldsCorrectly()
    {
        // Arrange
        var dto = new LabDto
        {
            created_at = new Created_At
            {
                date_utc = "2025-02-18T12:34:56Z"
            },
            display = new Display
            {
                text = "Test contents"
            },
            entities = [new Entities
            {
                buyer = new Buyer
                {
                    telephone = "2345678901"
                }
            }],
            metadata = new Metadata
            {
                location = new LabLocation
                {
                    name = "Dallas"
                }
            }
        };

        // Act
        var result = _translator.Translate(dto);

        // Assert
        Assert.Equal(0, result.Id);
        Assert.Equal("Test contents", result.Contents);
        Assert.Equal("Unknown", result.Branch);
        Assert.Equal("Location: Dallas", result.MetaData);
        Assert.Equal(Source.Lab, result.Source);

        Assert.Equal(
            DateTimeOffset.Parse("2025-02-18T12:34:56Z"),
            result.Date
        );

        Assert.Equal(2345678901, result.PhoneNumber.Number);
    }

    [Fact]
    public void Translate_InvalidPhone_UsesDefaultPhone()
    {
        var dto = new LabDto
        {
            entities = [new Entities
            {
                buyer = new Buyer
                {
                    telephone = "not-a-phone"
                }
            }]
        };

        var result = _translator.Translate(dto);

        Assert.Equal(PhoneNumber.Default, result.PhoneNumber.Number);
    }

    [Fact]
    public void Translate_InvalidDate_UsesMinValue()
    {
        var dto = new LabDto
        {
            created_at = new Created_At
            {
                date_utc = "invalid-date"
            }
        };

        var result = _translator.Translate(dto);

        Assert.Equal(DateTimeOffset.MinValue, result.Date);
    }

    [Fact]
    public void Translate_NullText_SetsEmptyString()
    {
        var dto = new LabDto
        {
            display = new Display
            {
                text = null
            }
        };

        var result = _translator.Translate(dto);

        Assert.Equal(string.Empty, result.Contents);
    }

    [Fact]
    public void Translate_NullLocation_SetsUnknown()
    {
        var dto = new LabDto
        {
            metadata = new Metadata
            {
                location = null
            }
        };

        var result = _translator.Translate(dto);

        Assert.Equal("Location: Unknown", result.MetaData);
    }
}
