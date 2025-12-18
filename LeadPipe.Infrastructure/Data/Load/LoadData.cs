using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Data.Load;

public abstract class LoadData<TVo, TEntity>(
    IRepository<TEntity> repo,
    IEntityToVo<TEntity, TVo> eToVo,
    Domain.ValueObjects.Source source
    ) : ILoadData<TVo> where TEntity : class, ISourceEntity
{
    private readonly IRepository<TEntity> _repo = repo;
    private readonly IEntityToVo<TEntity, TVo> _eToVo = eToVo;
    private readonly Domain.ValueObjects.Source _source = source;
    public async Task<Result<List<TVo>>> LoadAsync()
    {
        try
        {
            Result<List<TEntity>> result = await _repo.FindAsync(e => e.Source == _source);
            return result.IsSuccess
                ? Result.Success(result.Value.Select(_eToVo.Translate).ToList())
                : Result.Failure<List<TVo>>(result.Error);
        }
        catch (Exception ex) { return Result.Failure<List<TVo>>(ex.Message); }
    }
}
