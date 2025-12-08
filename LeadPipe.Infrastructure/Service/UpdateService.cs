using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Service;

public abstract class UpdateService<TDto, TVo, TEntity>(
    IDataSourceAsync<TDto> source,
    IDtoToVo<TDto, TVo> dtoToVo,
    IVoToEntity<TVo, TEntity> voToEntity,
    IDataPersistence<TEntity> persistence
    ) : IUpdateService<TVo>
{
    private readonly IDataSourceAsync<TDto> _source = source;
    private readonly IDtoToVo<TDto, TVo> _dtoToVo = dtoToVo;
    private readonly IVoToEntity<TVo, TEntity> _voToEntity = voToEntity;
    private readonly IDataPersistence<TEntity> _persistence = persistence;
    public async Task<Result<List<TVo>>> GetDataAsync()
    {
        Result<List<TDto>> raw = await _source.LoadAsync();
        if (raw.IsFailure)
            return Result.Failure<List<TVo>>(raw.Error);
        List<TVo> result = [.. raw.Value.Select(_dtoToVo.Translate)];
        return result;
    }
    public async Task<Result<List<TVo>>> UpdateDataAsync()
    {
        Result<List<TDto>> raw = await _source.RefreshAsync();
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
