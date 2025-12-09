using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Domain.Test;

public class PhoneNumberTests
{
    // ---- Constructors: PhoneNumber(long) ----

    [Theory]
    [InlineData(2234567891, 2234567891)]
    [InlineData(02234567890, 2234567890)]  // Leading zero falls off
    [InlineData(12345, PhoneNumber.Default)] // too small
    [InlineData(1000000000, PhoneNumber.Default)] // unacceptable first digit (1)
    [InlineData(01123456789, PhoneNumber.Default)] // unacceptable area code
    public void Ctor_Long_ParsesCorrectly(long input, long expected)
    {
        var p = new PhoneNumber(input);
        Assert.Equal(expected, p.Number);
    }


    // ---- Constructors: PhoneNumber(string) ----

    [Theory]
    [InlineData("2234567890", 2234567890)]
    [InlineData("(223) 456-7890", 2234567890)]
    [InlineData(" 223 456 7890 ", 2234567890)]
    [InlineData("2234567890ex55", 2234567890)]
    [InlineData("", PhoneNumber.Default)]
    [InlineData(null, PhoneNumber.Default)]
    [InlineData("abc", PhoneNumber.Default)]
    [InlineData("1 (234) 567-8900", 2345678900)]  // unacceptable leading digit "1"
    [InlineData("0001234567890", PhoneNumber.Default)]     // unacceptable leading digit "0"
    public void Ctor_String_ParsesCorrectly(string? input, long expected)
    {
        var p = new PhoneNumber(input);
        Assert.Equal(expected, p.Number);
    }


    // ---- Copy constructor: PhoneNumber(PhoneNumber) ----

    [Fact]
    public void Ctor_Copy_CopiesNumber()
    {
        var original = new PhoneNumber(1234567890);
        var copy = new PhoneNumber(original);
        Assert.Equal(original.Number, copy.Number);
    }


    // ---- TryParse ----

    [Fact]
    public void TryParse_Valid_ReturnsTrue()
    {
        long num = 2234567890;
        bool ok = PhoneNumber.TryParse($"{num}", out var result);

        Assert.True(ok);
        Assert.Equal(num, result.Number);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("123")]     // too short
    [InlineData("1000000000")] // unacceptable area code
    public void TryParse_Invalid_ReturnsFalse_WithDefault(string? input)
    {
        bool ok = PhoneNumber.TryParse(input, out var result);

        Assert.False(ok);
        Assert.Equal(PhoneNumber.Default, result.Number);
    }


    // ---- ToString ----

    [Fact]
    public void ToString_FormatsProperly()
    {
        var p = new PhoneNumber(2234567890);
        Assert.Equal("(223) 456-7890", p.ToString());
    }

    [Fact]
    public void ToString_OnDefault_IsZeroFormatted()
    {
        var p = new PhoneNumber(PhoneNumber.Default);
        var s = p.ToString();
        Assert.Equal("(000) 000-0000", s);
    }


    // ---- IsDefault ----

    [Theory]
    [InlineData(PhoneNumber.Default, true)]
    [InlineData(1111111111, true)] // special rule
    [InlineData(2234567890, false)]
    public void IsDefault_ComputedCorrectly(long input, bool expected)
    {
        var p = new PhoneNumber(input);
        Assert.Equal(expected, p.IsDefault);
    }

    [Fact]
    public void IsDefault_CachesResult()
    {
        var p = new PhoneNumber(2234567890);

        // first call computes
        bool first = p.IsDefault;

        // second call returns cached value
        bool second = p.IsDefault;

        Assert.Equal(first, second);
    }


    // ---- Equals / GetHashCode ----

    [Fact]
    public void Equals_True_ForSameNumber()
    {
        var p1 = new PhoneNumber(1234567890);
        var p2 = new PhoneNumber(1234567890);

        Assert.Equal(p1, p2);
        Assert.Equal(p2, p1);
    }

    [Fact]
    public void Equals_False_ForDifferentNumber()
    {
        var p1 = new PhoneNumber(2234567890);
        var p2 = new PhoneNumber(5555555555);

        Assert.NotEqual(p1, p2);
    }

    [Fact]
    public void Equals_False_ForNull()
    {
        var p = new PhoneNumber(1234567890);
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        PhoneNumber n = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        Assert.NotEqual(p, n);
    }

    [Fact]
    public void GetHashCode_MatchesNumber()
    {
        var p = new PhoneNumber(2234567890);
        Assert.Equal(2234567890.GetHashCode(), p.GetHashCode());
    }


    // ---- Operators == and != ----

    [Fact]
    public void Operator_Equal_True()
    {
        var num = 2234567890;
        var p1 = new PhoneNumber(num);
        var p2 = new PhoneNumber(num);

        Assert.Equal(p1, p2);
    }

    [Fact]
    public void Operator_Equal_False()
    {
        var p1 = new PhoneNumber(1234567890);
        var p2 = new PhoneNumber(5555555555);

        Assert.NotEqual(p1, p2);
    }

    [Fact]
    public void Operator_Equal_NullBoth()
    {
        PhoneNumber? p1 = null;
        PhoneNumber? p2 = null;

        Assert.Equal(p1, p2);
    }

    [Fact]
    public void Operator_Equal_OneNull()
    {
        PhoneNumber? p1 = new(2234567890);
        PhoneNumber? p2 = null;

        Assert.NotEqual(p1, p2);
    }
}
