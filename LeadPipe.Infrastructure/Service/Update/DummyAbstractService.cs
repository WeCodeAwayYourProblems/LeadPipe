using CSharpFunctionalExtensions;
using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Infrastructure.Service.Update;

internal abstract class DummyAbstractService : IUpdateService<Plumbing>
{
    public Task<Result<List<Plumbing>>> GetDataAsync()
    {
        return Task.FromResult(Result.Success<List<Plumbing>>([]));
    }

    public Task<Result> SaveDataAsync(List<Plumbing> data)
    {
        return Task.FromResult(Result.Success());
    }

    public Task<Result<List<Plumbing>>> UpdateDataAsync()
    {
        return Task.FromResult(Result.Success<List<Plumbing>>([]));
    }
}
[SourceKey(Source.Test)]
internal sealed class DummyUpdateService : DummyAbstractService, IUpdateService<Plumbing> { }
[SourceKey(Source.Test2)]
internal sealed class DummyUpdateService2 : DummyAbstractService, IUpdateService<Plumbing> { }