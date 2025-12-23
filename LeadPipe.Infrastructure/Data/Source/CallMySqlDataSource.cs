using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.MySql;

namespace LeadPipe.Infrastructure.Data.Source;

public sealed class CallMySqlDataSource(
    ICallMySqlRepository repo
    ) : IDataSourceAsync<CallMySqlEntity>
{
    private readonly ICallMySqlRepository _repo = repo;
    public async Task<Result<List<CallMySqlEntity>>> LoadAsync()
    {
        DateTime dateFilter = DateTime.UtcNow.AddYears(-2);
        Result<List<CallMySqlEntity>> found = await _repo.FindAsync(c => c.called_at_utc <= dateFilter, true);
        return found;
    }

    public Task<Result<List<CallMySqlEntity>>> RefreshAsync()
    {
        throw new NotImplementedException();
    }
}