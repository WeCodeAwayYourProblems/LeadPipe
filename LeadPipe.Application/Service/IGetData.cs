using CSharpFunctionalExtensions;

namespace LeadPipe.Application.Service;

public interface IGetData<T>
{
    Task<Result<List<T>>> GetDataAsync();
}
