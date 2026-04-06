using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Service.Update;

internal abstract class ValueObjectUpdateService<TEntity, TVo>(
    IDataSourceAsync<TEntity> source,
    IEntityToVo<TEntity, TVo> eToVo,
    IDataPersistence<TVo> persistence,
    SyncKey key
    ) : IUpdateService<TVo>
{
    private readonly IDataSourceAsync<TEntity> _source = source;
    private readonly IEntityToVo<TEntity, TVo> _eToVo = eToVo;
    private readonly IDataPersistence<TVo> _persistence = persistence;

    public SyncKey SyncKey => key;

    public Task<Result<List<TVo>>> GetDataAsync() => GetDataAsync(false);
    public async Task<Result<List<TVo>>> GetDataAsync(bool withDetails)
    {
        // Retrieve all data from source
        Result<List<TEntity>> result = await _source.LoadAsync(withDetails);
        if (result.IsFailure)
            return Result.Failure<List<TVo>>(result.Error);

        Result<List<TVo>> value = result.IsSuccess
            ? Result.Success(result.Value.Select(_eToVo.Translate).ToList())
            : Result.Failure<List<TVo>>(result.Error);
        return value;
    }

    public async Task<Result> SaveDataAsync(List<TVo> data)
    {
        Result saved = await _persistence.SaveAsync(data);
        return saved;
    }

    public async Task<Result<List<TVo>>> UpdateDataAsync(bool withDetails)
    {
        Result<List<TEntity>> refresh = await _source.RefreshAsync(withDetails);

        Result<List<TVo>> result = refresh.IsSuccess
            ? Result.Success(refresh.Value.Select(_eToVo.Translate).ToList())
            : Result.Failure<List<TVo>>(refresh.Error);
        return result;
    }
}
