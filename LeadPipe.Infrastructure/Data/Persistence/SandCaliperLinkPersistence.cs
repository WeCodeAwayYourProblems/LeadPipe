using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal class SandCaliperLinkPersistence(IRepository<SandCaliperLink> repo) : Persistence<IRepository<SandCaliperLink>, SandCaliperLink>(repo), IDataPersistence<SandCaliperLink>{ }
