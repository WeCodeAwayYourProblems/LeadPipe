using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Service;

internal abstract class ValueObjectUpdateService<TEntity, TVo>(
    IDataSourceAsync<TEntity> source,
    IEntityToVo<TEntity, TVo> eToVo,
    IDataPersistence<TVo> persistence
    ) : IUpdateService<TVo>
{
    private readonly IDataSourceAsync<TEntity> _source = source;
    private readonly IEntityToVo<TEntity, TVo> _eToVo = eToVo;
    private readonly IDataPersistence<TVo> _persistence = persistence;
    public async Task<Result<List<TVo>>> GetDataAsync()
    {
        // Retrieve all data from source
        Result<List<TEntity>> callsResult = await _source.LoadAsync();
        if (callsResult.IsFailure)
            return Result.Failure<List<TVo>>(callsResult.Error);

        Result<List<TVo>> value = callsResult.IsSuccess
            ? Result.Success(callsResult.Value.Select(_eToVo.Translate).ToList())
            : Result.Failure<List<TVo>>(callsResult.Error);
        return value;
    }

    public async Task<Result> SaveDataAsync(List<TVo> data)
    {
        Result saved = await _persistence.SaveAsync(data);
        return saved;
    }

    public async Task<Result<List<TVo>>> UpdateDataAsync()
    {
        Result<List<TEntity>> callsResult = await _source.RefreshAsync();
        if (callsResult.IsFailure)
            return Result.Failure<List<TVo>>(callsResult.Error);

        return callsResult.IsSuccess
            ? Result.Success(callsResult.Value.Select(_eToVo.Translate).ToList())
            : Result.Failure<List<TVo>>(callsResult.Error);
    }
}
