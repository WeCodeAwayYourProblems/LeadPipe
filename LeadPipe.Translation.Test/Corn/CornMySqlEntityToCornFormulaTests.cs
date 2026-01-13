using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Translation.Translate.EntityToVo;

namespace LeadPipe.Translation.Test.Corn;

public sealed class CornMySqlEntityToCornFormulaTests
{
    private static CornMySqlEntity CreateEntity(string? phone = "5551239999")
        => new()
        {
            id = 77,
            phoneNumber = phone,
            timestamp = new DateTime(2025, 6, 1, 12, 0, 0),
            comments = "Needs service",
            form = "ContactUs",
            referringURL = "google",
            source = "corn"
        };

    [Fact]
    public void Translate_ShouldProduceUtcDateOffsetZero()
    {
        var entity = CreateEntity();
        var translator = new CornMySqlEntityToCornFormula();

        var vo = translator.Translate(entity);

        Assert.Equal(TimeSpan.Zero, vo.Date.Offset);
        Assert.Equal(
            DateTime.SpecifyKind(entity.timestamp, DateTimeKind.Utc),
            vo.Date.UtcDateTime
        );
    }

    [Fact]
    public void Translate_ShouldMapPhoneCorrectly()
    {
        var entity = CreateEntity();
        var translator = new CornMySqlEntityToCornFormula();

        var vo = translator.Translate(entity);

        Assert.Equal(5551239999, vo.PhoneNumber.Number);
    }

    [Fact]
    public void Translate_InvalidPhone_ShouldFallbackToDefault()
    {
        var entity = CreateEntity(phone: "not a phone");
        var translator = new CornMySqlEntityToCornFormula();

        var vo = translator.Translate(entity);

        Assert.Equal(PhoneNumber.Default, vo.PhoneNumber.Number);
    }

    [Fact]
    public void Translate_ShouldBuildPayloadAndMetadataCorrectly()
    {
        var entity = CreateEntity();
        var translator = new CornMySqlEntityToCornFormula();

        var vo = translator.Translate(entity);

        Assert.Equal("Needs service", vo.PayLoad);
        Assert.Equal("Form: ContactUs | Referring: google", vo.MetaData);
        Assert.Equal("corn", vo.Source);
    }

    [Fact]
    public void Translate_ShouldRemainIdempotent_AtScale()
    {
        var entity = CreateEntity();
        var translator = new CornMySqlEntityToCornFormula();

        CornFormula current = translator.Translate(entity);

        for (int i = 0; i < 1_000_000; i++)
        {
            current = translator.Translate(entity);
        }

        Assert.Equal(77, current.Id);
        Assert.Equal(TimeSpan.Zero, current.Date.Offset);
        Assert.Equal(5551239999, current.PhoneNumber.Number);
        Assert.Equal("Needs service", current.PayLoad);
    }
}
