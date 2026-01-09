using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal sealed class CaliperEntityPersistence(ICaliperRepository repo) : Persistence<ICaliperRepository, CaliperEntity>(repo), IDataPersistence<CaliperEntity> { }
