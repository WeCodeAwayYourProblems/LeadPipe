using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal sealed class SubsEntityPersistence(ISubsRepository repo) : Persistence<ISubsRepository, SubsEntity>(repo), IDataPersistence<SubsEntity> { }