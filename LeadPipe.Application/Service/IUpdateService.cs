using CSharpFunctionalExtensions;

namespace LeadPipe.Application.Service;

public interface IUpdateService<TVo>
{
    Task<Result<List<TVo>>> GetDataAsync();
    Task<Result> SaveDataAsync(List<TVo> data);
}
