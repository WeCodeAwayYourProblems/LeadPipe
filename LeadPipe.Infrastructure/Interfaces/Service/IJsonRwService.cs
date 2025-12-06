using CSharpFunctionalExtensions;

namespace LeadPipe.Infrastructure.Interfaces.Service;

public interface IJsonRwService
{
    Result<List<T>> ReadFile<T>(FileInfo path);
    Result WriteToFile<T>(FileInfo path, List<T> items);
}
