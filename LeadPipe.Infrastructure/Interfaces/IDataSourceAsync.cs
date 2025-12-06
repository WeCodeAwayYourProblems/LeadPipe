using CSharpFunctionalExtensions;

namespace LeadPipe.Infrastructure.Interfaces;

public interface IDataSourceAsync<T>:ILoad<T>
{
    Task<Result<List<T>>> RefreshAsync();
}
