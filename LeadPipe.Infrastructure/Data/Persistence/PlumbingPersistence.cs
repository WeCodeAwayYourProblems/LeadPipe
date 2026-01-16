using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal class PlumbingPersistence(IRepository<PlumbingEntity> repo) : Persistence<IRepository<PlumbingEntity>, PlumbingEntity>(repo), IDataPersistence<PlumbingEntity> { }
