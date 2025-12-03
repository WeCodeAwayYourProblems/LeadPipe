using CSharpFunctionalExtensions;

namespace LeadPipe.Infrastructure.Data.Source;

public interface IDataSourceAsync<TDto>
{
    public Task<Result<List<TDto>>> LoadAsync();
}
