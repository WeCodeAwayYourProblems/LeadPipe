using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces;
using LeadPipe.Infrastructure.Interfaces.Repository.MySql;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal class CustomerMySqlEntityPersistence(ICustomerMySqlRepository repo) : IDataPersistence<CustomerMySqlEntity>
{
    private readonly ICustomerMySqlRepository _repo = repo; 
    public async Task<Result> SaveAsync(List<CustomerMySqlEntity> t)
    {
        Result<List<CustomerMySqlEntity>> added = await _repo.AddRangeAsync(t); return added;
    }
}
