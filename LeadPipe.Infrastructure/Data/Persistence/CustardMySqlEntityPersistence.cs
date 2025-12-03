using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Repository;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal class CustardMySqlEntityPersistence(ICustardMySqlRepository repo) : IDataPersistence<CustardMySqlEntity>
{
    private readonly ICustardMySqlRepository _repo = repo; 
    public async Task<Result> SaveAsync(List<CustardMySqlEntity> t)
    {
        Result<List<CustardMySqlEntity>> added = await _repo.AddRangeAsync(t); return added;
    }
}
