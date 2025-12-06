using CSharpFunctionalExtensions;

namespace LeadPipe.Infrastructure.Interfaces;

public interface ILoad<T>
{
    Task<Result<List<T>>> LoadAsync();
}
