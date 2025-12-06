using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal class CallEntityPersistence(ICallRepository repo) : Persistence<ICallRepository, CallEntity>(repo), IDataPersistence<CallEntity>{ }
