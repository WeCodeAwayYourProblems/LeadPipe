using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal sealed class CornEntityPersistence(IRepository<CornEntity> repo): Persistence<IRepository<CornEntity>, CornEntity>(repo), IDataPersistence<CornEntity> { }