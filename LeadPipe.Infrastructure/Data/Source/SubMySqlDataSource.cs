using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.MySql;

namespace LeadPipe.Infrastructure.Data.Source;

public sealed class SubMySqlDataSource(
    ISubMySqlRepository repo
    ) : IDataSourceAsync<SubMySqlEntity>
{
    private readonly ISubMySqlRepository _repo = repo;
    public async Task<Result<List<SubMySqlEntity>>> LoadAsync()
    {
        DateTime twentyTwelve = new(new DateOnly(2012, 1, 1), new TimeOnly(0), DateTimeKind.Utc);
        Result<List<SubMySqlEntity>> found = await _repo.FindAsync(s => s.dateAdded <= twentyTwelve, true);
        return found;
    }

    public async Task<Result<List<SubMySqlEntity>>> RefreshAsync()
    {
        return await LoadAsync();
    }
}