using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal class PlumbingCaliperLinkPersistence(IRepository<PlumbingCaliperLink> repo) 
    : Persistence<IRepository<PlumbingCaliperLink>, PlumbingCaliperLink>(repo), IDataPersistence<PlumbingCaliperLink>{ }
