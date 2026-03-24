using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.MySql;

namespace LeadPipe.Infrastructure.Data.DataSource;

public sealed class SandMySqlDataSource(
    ISandMySqlRepository repo
) : IDataSourceAsync<SandMySqlEntity>
{
    private readonly ISandMySqlRepository _repo = repo;
    public async Task<Result<List<SandMySqlEntity>>> LoadAsync(bool withDetails)
    {
        DateTime twentyTwelve = new(new DateOnly(2012, 1, 1), new TimeOnly(0), DateTimeKind.Utc);
        Result<List<SandMySqlEntity>> found = await _repo.FindAsync(s => s.dateAdded >= twentyTwelve, withDetails);
        return found;
    }

    public async Task<Result<List<SandMySqlEntity>>> RefreshAsync(bool withDetails) => await LoadAsync(withDetails);
}
