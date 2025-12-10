using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal class PlumbingPersistence(IPlumbingRepository repo) : Persistence<IPlumbingRepository, PlumbingEntity>(repo), IDataPersistence<PlumbingEntity> { }
