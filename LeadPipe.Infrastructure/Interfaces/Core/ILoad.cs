using CSharpFunctionalExtensions;

namespace LeadPipe.Infrastructure.Interfaces.Core;

public interface ILoad<T>
{
    Task<Result<List<T>>> LoadAsync();
}
