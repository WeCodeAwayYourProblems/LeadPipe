using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.MySql;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal class SummaryMySqlEntityPersistence(ISummaryMySqlRepository repo) : IDataPersistence<SummaryMySqlEntity>
{
    private readonly ISummaryMySqlRepository _repo = repo; 
    public async Task<Result> SaveAsync(List<SummaryMySqlEntity> t)
    {
        Result<List<SummaryMySqlEntity>> added = await _repo.AddRangeAsync(t); return added;
    }
}