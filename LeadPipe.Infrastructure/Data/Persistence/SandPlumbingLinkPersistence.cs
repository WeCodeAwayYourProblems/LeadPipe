using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal class SandPlumbingLinkPersistence(IRepository<SandPlumbingLink> repo) : Persistence<IRepository<SandPlumbingLink>, SandPlumbingLink>(repo), IDataPersistence<SandPlumbingLink>{ }
