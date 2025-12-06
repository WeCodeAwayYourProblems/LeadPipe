using CSharpFunctionalExtensions;

namespace LeadPipe.Infrastructure.Interfaces.Core;

public interface IDataPersistence<T>
{
    Task<Result> SaveAsync(List<T> t);
}
