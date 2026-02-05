namespace LeadPipe.Infrastructure.Interfaces.Core;

public interface ISourceEntity : IEntity
{
    Domain.ValueObjects.Source Source { get; set; }
}