using CSharpFunctionalExtensions;

namespace LeadPipe.Infrastructure.Data.Persistence;

public interface IDataPersistence<T>
{
    Task<Result> SaveAsync(List<T> t);
}
