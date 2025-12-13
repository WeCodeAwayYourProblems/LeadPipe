using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Data.Source;

[SourceKey(Domain.ValueObjects.Source.Yeller)]
public class YellerLoadData(
    IPlumbingRepository plumb,
    IEntityToVo<PlumbingEntity, Plumbing> eToPlumb
    ) : ILoadData<Plumbing>
{
    private readonly IPlumbingRepository _plumb = plumb;
    private readonly IEntityToVo<PlumbingEntity, Plumbing> _eToPlumb = eToPlumb;
    public async Task<Result<List<Plumbing>>> LoadAsync()
    {
        try
        {
            Result<List<PlumbingEntity>> result = await _plumb.GetAllAsync(source: Domain.ValueObjects.Source.Yeller);
            return result.IsSuccess
                ? Result.Success(result.Value.Select(_eToPlumb.Translate).ToList())
                : Result.Failure<List<Plumbing>>(result.Error);
        }
        catch (Exception ex) { return Result.Failure<List<Plumbing>>(ex.Message); }
    }
}