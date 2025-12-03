using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Repository;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal class PlumbingCallLinkPersistence(IPlumbingCallLinkRepository repo) : Persistence<IPlumbingCallLinkRepository, PlumbingCallLink>(repo), IDataPersistence<PlumbingCallLink>{ }
