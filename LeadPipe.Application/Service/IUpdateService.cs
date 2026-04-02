using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Application.Service;

public interface IUpdateService<TVo> : IGetData<TVo>
{
    Task<Result<List<TVo>>> UpdateDataAsync(bool withDetails);
    Task<Result> SaveDataAsync(List<TVo> data);
    SyncKey SyncKey { get; }
}
