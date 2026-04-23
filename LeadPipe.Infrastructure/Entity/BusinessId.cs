using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Infrastructure.Entity;

public readonly record struct BusinessId(string Value)
{
    public string Value { get; } = Value;
    public static BusinessId From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("BusinessId cannot be empty");

        return new BusinessId(value);
    }

    public override string ToString() => Value;

    public static BusinessId BuildBusinessId(Source? source, SyncKey entity)
    {
        string scope = source is null
            ? "global"
            : source.ToString()!.ToLowerInvariant();

        return From($"{scope}:{entity}");
    }
}
