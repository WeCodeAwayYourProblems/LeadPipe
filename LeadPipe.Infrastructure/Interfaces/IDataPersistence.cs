using CSharpFunctionalExtensions;

namespace LeadPipe.Infrastructure.Interfaces;

public interface IDataPersistence<T>
{
    Task<Result> SaveAsync(List<T> t);
}
