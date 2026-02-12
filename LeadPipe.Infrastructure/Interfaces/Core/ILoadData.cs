using CSharpFunctionalExtensions;

namespace LeadPipe.Infrastructure.Interfaces.Core;

public interface ILoadData<T>
{
    Task<Result<List<T>>> LoadAsync(bool withDetails);
}
