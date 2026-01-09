using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal class PlumbingCaliperLinkPersistence(IPlumbingCaliperLinkRepository repo) : Persistence<IPlumbingCaliperLinkRepository, PlumbingCaliperLink>(repo), IDataPersistence<PlumbingCaliperLink>{ }
