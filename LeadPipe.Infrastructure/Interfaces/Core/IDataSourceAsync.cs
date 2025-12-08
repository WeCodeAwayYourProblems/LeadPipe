using CSharpFunctionalExtensions;

namespace LeadPipe.Infrastructure.Interfaces.Core;

public interface IDataSourceAsync<T>
{
    Task<Result<List<T>>> LoadAsync();
    Task<Result<List<T>>> RefreshAsync();
}
