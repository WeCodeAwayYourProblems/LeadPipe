using CSharpFunctionalExtensions;

namespace LeadPipe.Infrastructure.Interfaces.Core;

public interface IDataSourceAsync<T>:ILoad<T>
{
    Task<Result<List<T>>> RefreshAsync();
}
