using CSharpFunctionalExtensions;

namespace LeadPipe.Application.Service;

public interface IUpdateService<TVo> : IGetData<TVo>
{
    Task<Result<List<TVo>>> UpdateDataAsync();
    Task<Result> SaveDataAsync(List<TVo> data);
}
