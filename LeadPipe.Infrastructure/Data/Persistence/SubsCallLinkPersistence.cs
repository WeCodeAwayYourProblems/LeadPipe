using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal class SubsCallLinkPersistence(ISubsCallLinkRepository repo) : Persistence<ISubsCallLinkRepository, CallSubsLink>(repo), IDataPersistence<CallSubsLink>{ }
