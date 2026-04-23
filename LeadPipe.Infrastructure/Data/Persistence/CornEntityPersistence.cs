using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal sealed class CornEntityPersistence(IRepository<CornEntity> repo): Persistence<IRepository<CornEntity>, CornEntity>(repo), IDataPersistence<CornEntity> { }