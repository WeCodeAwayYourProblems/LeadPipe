using LeadPipe.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LeadPipe.Infrastructure.Database.Configuration;

internal static class PlumbingConversionsHelper
{
    public static readonly ValueComparer<PhoneNumber> PhoneNumberComparer = new(
        (a, b) =>
            ReferenceEquals(a, b) ||
            (a != null && b != null && a.Number == b.Number),
        p => p == null || p.Number == PhoneNumber.Default
            ? 0
            : p.Number.GetHashCode(),
        p => p == null ? null! : new PhoneNumber(p.Number)
    );
    public static readonly ValueComparer<PhoneNumber?> NullablePhoneNumberComparer = new(
        (a, b) =>
            ReferenceEquals(a, b) ||
            (a != null && b != null && a.Number == b.Number),
        p => p == null || p.Number == PhoneNumber.Default
            ? 0
            : p.Number.GetHashCode(),
        p => p == null ? null : new PhoneNumber(p.Number)
    );
    public static readonly ValueConverter<PhoneNumber, long> PhoneNumberAndLongConversion = new(
        p => p.Number,
        v => new PhoneNumber(v)
    );
    public static readonly ValueConverter<PhoneNumber?, long?> PhoneNumberNullableConverter = new(
        p => p == null ? null : p.Number,
        v => v == null ? null : new PhoneNumber(v.Value)
    );
}