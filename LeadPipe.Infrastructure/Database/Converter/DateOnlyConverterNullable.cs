using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;

namespace LeadPipe.Infrastructure.Database.Converter;

internal sealed class DateOnlyConverterNullable : ValueConverter<DateOnly?, string?>
{
    public DateOnlyConverterNullable()
        : base(
            d => d == null ? null : d.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            s => s == null ? null : DateOnly.ParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture))
    { }
}