using System.Text.RegularExpressions;

namespace LeadPipe.Domain.ValueObjects;

public partial class PhoneNumber
{
    #region Public
    public override string ToString()
    {
        // Convert the number to a string, but don't lose leading zeros
        string digits = Number.ToString().PadLeft(10, '0');

        // Take only the last 10 digits (if Number was longer)
        digits = digits.Length > 10 ? digits[^10..] : digits;

        // Manually insert formatting characters
        return $"({digits[0..3]}) {digits[3..6]}-{digits[6..10]}";
    }

    public bool CanParticipateInDeduplication => !IsDefault && !IsJunk;
    public bool IsDefault => Number == Default;
    public bool IsJunk => Number == 1111111111 || Number == 5555555555;

    public const long Default = 0;
    public readonly static PhoneNumber DefaultPhoneNumber = new(Default);

    public long Number { get; }

    public PhoneNumber(PhoneNumber number)
    {
        Number = number.Number;
    }

    public PhoneNumber(long number)
    {
        Number = ValidateNumericalInput(number);
    }

    public PhoneNumber(string? number)
    {
        Number = ValidateStringInput(number);
    }

    public static bool TryParse(string? number, out PhoneNumber result)
    {
        result = new(number);
        return result.Number != Default;
    }

    public static bool TryParseMany(string? input, out List<PhoneNumber> results)
    {
        results = [];

        if (string.IsNullOrWhiteSpace(input))
            return false;
        HashSet<long> seen = [];
        foreach (Match match in PhoneMatchRegex().Matches(input))
        {
            if (TryParse(match.Value, out var parsed) && seen.Add(parsed.Number))
                results.Add(parsed);
        }

        if (results.Count == 0)
            return false;

        return true;
    }

    #endregion

    #region Private
    static long ValidateStringInput(string? number)
    {
        if (string.IsNullOrWhiteSpace(number))
            return Default;

        number = ExtensionRegex().Replace(number, "");

        string clean = NonDigitChar().Replace(number, string.Empty);

        return StrToLong(clean);
    }

    static long ValidateNumericalInput(long number)
    {
        return StrToLong(number.ToString());
    }

    static long StrToLong(string number)
    {
        if (number.Length < 10)
            return Default;

        // Normalize to 10 digits
        if (number.Length == 11 && number.StartsWith(value: '1'))
            number = number[1..];
        else if (number.Length > 10)
            number = number[^10..];

        if (!long.TryParse(number, out long result))
            return Default;

        // NANP validation
        if (number[0] is '0' or '1') return Default; // area code
        if (number[3] is '0' or '1') return Default; // exchange

        return result;
    }


    [GeneratedRegex(@"\D")]
    private static partial Regex NonDigitChar();

    [GeneratedRegex(@"\s*(x|ext\.?|#)\s*\d+", RegexOptions.IgnoreCase)]
    private static partial Regex ExtensionRegex();

    [GeneratedRegex(
    @"(?:\+?1[\s.-]?)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}(?:\s*(?:x|ext\.?|#)\s*\d+)?",
    RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex PhoneMatchRegex();


    #endregion

    #region Equality

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj is not PhoneNumber) return false;
        return Equals(obj as PhoneNumber);
    }

    public bool Equals(PhoneNumber? that)
    {
        if (ReferenceEquals(this, that)) return true;
        if (that is null) return false;

        return Number == that.Number;
    }

    public override int GetHashCode()
    {
        return Number.GetHashCode();
    }

    public static bool operator ==(PhoneNumber? left, PhoneNumber? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(PhoneNumber? left, PhoneNumber? right)
    {
        return !(left == right);
    }

    #endregion
}
