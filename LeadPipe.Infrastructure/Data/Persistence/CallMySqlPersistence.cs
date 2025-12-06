using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces;
using LeadPipe.Infrastructure.Repository;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal class CallMySqlPersistence(ICallMySqlRepository repo) : IDataPersistence<CallMySqlEntity>
{
    private readonly ICallMySqlRepository _repo = repo; 
    public async Task<Result> SaveAsync(List<CallMySqlEntity> t)
    {
        Result<List<CallMySqlEntity>> added = await _repo.AddRangeAsync(t); 
        return added;
    }
}
