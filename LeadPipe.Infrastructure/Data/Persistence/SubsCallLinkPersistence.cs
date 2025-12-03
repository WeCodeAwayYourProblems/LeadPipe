using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Repository;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal class SubsCallLinkPersistence(ISubsCallLinkRepository repo) : Persistence<ISubsCallLinkRepository, SubsCallLink>(repo), IDataPersistence<SubsCallLink>{ }
