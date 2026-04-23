using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal sealed class CustardEntityPersistence(IRepository<CustardEntity> repo)
    : Persistence<IRepository<CustardEntity>, CustardEntity>(repo), IDataPersistence<CustardEntity>
{ }