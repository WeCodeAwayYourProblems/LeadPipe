using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal class SubsCaliperLinkPersistence(IRepository<SandCaliperLink> repo) : Persistence<IRepository<SandCaliperLink>, SandCaliperLink>(repo), IDataPersistence<SandCaliperLink>{ }
