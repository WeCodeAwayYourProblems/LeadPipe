using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal sealed class SandEntityPersistence(IRepository<SandEntity> repo) : Persistence<IRepository<SandEntity>, SandEntity>(repo), IDataPersistence<SandEntity> { }