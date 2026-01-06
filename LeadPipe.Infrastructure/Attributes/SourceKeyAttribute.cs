using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Infrastructure.Attributes;

[AttributeUsage(AttributeTargets.Class)]
internal class SourceKeyAttribute(Source key) : Attribute, ISourceKeyAttribute
{
    public Source Key { get; } = key;
}
internal interface ISourceKeyAttribute
{
    public Source Key { get; }
}
