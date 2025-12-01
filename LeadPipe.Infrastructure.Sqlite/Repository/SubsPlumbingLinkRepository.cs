using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Repository;
using LeadPipe.Infrastructure.Sqlite.Context;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public class SubsPlumbingLinkRepository(PlumbingContext context) : PlumbingContextRepository<SubsPlumbingLink>(context), ISubsPlumbingLinkRepository { }
