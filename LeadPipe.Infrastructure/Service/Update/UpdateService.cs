using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Service.Update;

public abstract class UpdateService<TDto, TVo, TEntity>(
    IDataSourceAsync<TDto> source,
    IDtoToVo<TDto, TVo> dtoToVo,
    IVoToEntity<TVo, TEntity> voToEntity,
    IDataPersistence<TEntity> persistence,
    SyncKey key
    ) : IUpdateService<TVo>
{
    private readonly IDataSourceAsync<TDto> _source = source;
    private readonly IDtoToVo<TDto, TVo> _dtoToVo = dtoToVo;
    private readonly IVoToEntity<TVo, TEntity> _voToEntity = voToEntity;
    private readonly IDataPersistence<TEntity> _persistence = persistence;

    public SyncKey SyncKey => key;

    public Task<Result<List<TVo>>> GetDataAsync() => GetDataAsync(false);
    public async Task<Result<List<TVo>>> GetDataAsync(bool withDetails)
    {
        Result<List<TDto>> raw = await _source.LoadAsync(withDetails);
        if (raw.IsFailure)
            return Result.Failure<List<TVo>>(raw.Error);
        List<TVo> result = [.. raw.Value.Select(_dtoToVo.Translate)];
        return result;
    }

    public async Task<Result<List<TVo>>> UpdateDataAsync(bool withDetails)
    {
        Result<List<TDto>> raw = await _source.RefreshAsync(withDetails);
        if (raw.IsFailure)
            return Result.Failure<List<TVo>>(raw.Error);
        List<TVo> result = [.. raw.Value.Select(_dtoToVo.Translate)];
        return result;
    }

    public async Task<Result> SaveDataAsync(List<TVo> data)
    {
        List<TEntity> entities = [.. data.Select(_voToEntity.Translate)];
        Result result = await _persistence.SaveAsync(entities);
        return result;
    }

}
