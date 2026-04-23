using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Repository;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal class SandPlumbingLinkPersistence(IRepository<SandPlumbingLink> repo) : Persistence<IRepository<SandPlumbingLink>, SandPlumbingLink>(repo), IDataPersistence<SandPlumbingLink>{ }
