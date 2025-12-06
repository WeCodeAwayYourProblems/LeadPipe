using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces;
using LeadPipe.Infrastructure.Repository;

namespace LeadPipe.Infrastructure.Data.Persistence;

internal class SubsPlumbingLinkPersistence(ISubsPlumbingLinkRepository repo) : Persistence<ISubsPlumbingLinkRepository, SubsPlumbingLink>(repo), IDataPersistence<SubsPlumbingLink>{ }
