using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal sealed class CaliperEntityPersistence(IRepository<CaliperEntity> repo) : Persistence<IRepository<CaliperEntity>, CaliperEntity>(repo), IDataPersistence<CaliperEntity> { }
