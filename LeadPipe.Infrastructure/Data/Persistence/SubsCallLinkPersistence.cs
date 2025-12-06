using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces;
using LeadPipe.Infrastructure.Repository;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal class SubsCallLinkPersistence(ISubsCallLinkRepository repo) : Persistence<ISubsCallLinkRepository, CallSubsLink>(repo), IDataPersistence<CallSubsLink>{ }
