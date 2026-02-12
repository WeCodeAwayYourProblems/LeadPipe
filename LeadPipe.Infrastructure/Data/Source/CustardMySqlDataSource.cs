using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.MySql;

namespace LeadPipe.Infrastructure.Data.Source;

public sealed class CustardMySqlDataSource(
    ICustardMySqlRepository repo
) : IDataSourceAsync<CustardMySqlEntity>
{
    private readonly ICustardMySqlRepository _repo = repo;
    public async Task<Result<List<CustardMySqlEntity>>> LoadAsync(bool withDetails)
    {
        DateTime twentyTwelve = new(new DateOnly(2012, 1, 1), new TimeOnly(0), DateTimeKind.Utc);
        Result<List<CustardMySqlEntity>> found = await _repo.FindAsync(s => s.dateAdded >= twentyTwelve, withDetails);
        return found;
    }

    public async Task<Result<List<CustardMySqlEntity>>> RefreshAsync(bool withDetails)
    {
        DateTime lastMonth = DateTime.Now.AddDays(-30);
        Result<List<CustardMySqlEntity>> found = await _repo.FindAsync(s => s.dateAdded >= lastMonth, withDetails);
        return found;
    }
}
