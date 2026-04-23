using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal sealed class SandEntityPersistence(IRepository<SandEntity> repo)
    : Persistence<IRepository<SandEntity>, SandEntity>(repo), IDataPersistence<SandEntity>
{ }
