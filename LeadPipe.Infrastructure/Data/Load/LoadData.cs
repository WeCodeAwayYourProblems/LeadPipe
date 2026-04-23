using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository;
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
    public async Task<Result<List<TVo>>> LoadAsync(bool withDetails)
    {
        try
        {
            Result<List<TEntity>> found = withDetails
                ? await _repo.FindAsync(e => e.Source == _source)
                : await _repo.FindWithDetailsAsync(e => e.Source == _source);

            Result<List<TVo>> result = found.IsSuccess
                ? Result.Success<List<TVo>>([.. found.Value.Select(_eToVo.Translate)])
                : Result.Failure<List<TVo>>(found.Error);
            return result;
        }
        catch (Exception ex) { return Result.Failure<List<TVo>>(ex.ToString()); }
    }
}
