using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Attributes;

namespace LeadPipe.Infrastructure.Service.Update;

internal abstract class AbstractDummyUpdateService : IUpdateService<Plumbing>
{
    public Task<Result<List<Plumbing>>> GetDataAsync(bool _)
    {
        return Task.FromResult(Result.Success<List<Plumbing>>([]));
    }

    public Task<Result> SaveDataAsync(List<Plumbing> data)
    {
        return Task.FromResult(Result.Success());
    }

    public Task<Result<List<Plumbing>>> UpdateDataAsync(bool _)
    {
        return Task.FromResult(Result.Success<List<Plumbing>>([]));
    }
}
[SourceKey(Source.Test)]
internal sealed class DummyUpdateService : AbstractDummyUpdateService, IUpdateService<Plumbing> { }
[SourceKey(Source.Test2)]
internal sealed class DummyUpdateService2 : AbstractDummyUpdateService, IUpdateService<Plumbing> { }