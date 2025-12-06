using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal class PlumbingCallLinkPersistence(IPlumbingCallLinkRepository repo) : Persistence<IPlumbingCallLinkRepository, PlumbingCallLink>(repo), IDataPersistence<PlumbingCallLink>{ }
