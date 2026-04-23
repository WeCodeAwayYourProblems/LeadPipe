using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Translation.Translate.VoToEntity;

namespace LeadPipe.Translation.Test.CaliperTests;

public sealed class CaliperToCaliperMySqlEntityTests
{
    private static Caliper CreateVo(bool billable)
        => new(
            Id: 1,
            Date: new DateTimeOffset(2025, 3, 9, 2, 30, 0, TimeSpan.Zero),
            Number: new PhoneNumber(5559876543),
            Duration: TimeSpan.FromSeconds(125),
            Note: "mysql note",
            Source: "src",
            Label: "label",
            Location: "loc",
            Billable: billable
        );

    [Theory]
    [InlineData(true, "billable")]
    [InlineData(false, "non billable")]
    public void Translate_ShouldMapBillableCorrectly(bool input, string expected)
    {
        var vo = CreateVo(input);
        var translator = new CaliperToCaliperMySqlEntity();

        var entity = translator.Translate(vo);

        Assert.Equal(expected, entity.sale_billable);
    }

    [Fact]
    public void Translate_ShouldMapDateAsUtc()
    {
        var vo = CreateVo(true);
        var translator = new CaliperToCaliperMySqlEntity();

        var entity = translator.Translate(vo);

        Assert.Equal(vo.Date.UtcDateTime, entity.called_at_utc);
        Assert.Equal(DateTimeKind.Utc, entity.called_at_utc.Kind);
    }

    [Fact]
    public void Translate_ShouldMapAllFields()
    {
        var vo = CreateVo(true);
        var translator = new CaliperToCaliperMySqlEntity();

        var entity = translator.Translate(vo);

        Assert.Equal("5559876543", entity.contact_number_clean);
        Assert.Equal(125, entity.duration);
        Assert.Equal(vo.Source, entity.source);
        Assert.Equal(vo.Location, entity.location);
        Assert.Equal(vo.Note, entity.note);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10_000)]
    [InlineData(1_000_000)]
    public void Translate_ShouldRemainIdempotent_AtScale(int iterations)
    {
        var vo = CreateVo(true);
        var translator = new CaliperToCaliperMySqlEntity();

        CaliperMySqlEntity entity = translator.Translate(vo);

        for (int i = 0; i < iterations; i++)
        {
            entity = translator.Translate(vo);
        }

        Assert.Equal(vo.Date.UtcDateTime, entity.called_at_utc);
        Assert.Equal("5559876543", entity.contact_number_clean);
        Assert.Equal(125, entity.duration);
    }
}
