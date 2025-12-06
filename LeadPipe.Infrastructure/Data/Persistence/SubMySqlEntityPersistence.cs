using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.MySql;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal class SubMySqlEntityPersistence(ISubMySqlRepository repo) : IDataPersistence<SubMySqlEntity>
{
    private readonly ISubMySqlRepository _repo = repo; 
    public async Task<Result> SaveAsync(List<SubMySqlEntity> t)
    {
        Result<List<SubMySqlEntity>> added = await _repo.AddRangeAsync(t); return added;
    }
}
