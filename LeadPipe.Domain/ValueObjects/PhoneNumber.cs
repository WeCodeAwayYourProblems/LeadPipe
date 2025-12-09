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


    private bool? _isDefault;
    public bool IsDefault
    {
        get { return _isDefault ??= Number == Default || Number == 1111111111 || Number > 9999999999; }
    }

    public const long Default = 0;

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
        try
        {
            result = new(number);
            if (result.Number == Default) return false;
            return true;
        }
        catch { }
        result = new(Default);
        return false;
    }
    #endregion

    #region Private
    static long ValidateStringInput(string? number)
    {
        if (string.IsNullOrWhiteSpace(number))
            return Default;
        var split = number.Split("ex");
        if (split.Length == 0)
            return Default;

        string clean = NonDigitChar().Replace(split[0], string.Empty);
        long result = StrToLong(clean);
        return result;
    }

    static long ValidateNumericalInput(long number)
    {
        return StrToLong(number.ToString());
    }

    static readonly char[] _unacceptable = { '0', '1' };
    static long StrToLong(string number)
    {
        bool small = number.Length < 10;
        if (small) return Default;

        bool unacceptable = _unacceptable.Contains(number[^10]);
        bool parses = long.TryParse(number[^10..], out long result);

        if (parses && !unacceptable) return result;
        return Default;
    }

    [GeneratedRegex(@"\D")]
    private static partial Regex NonDigitChar();
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
