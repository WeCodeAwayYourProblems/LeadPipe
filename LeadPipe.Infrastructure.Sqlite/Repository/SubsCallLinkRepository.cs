using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Sqlite.Context;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public class SubsCallLinkRepository(PlumbingContext context) : PlumbingContextRepository<CallSubsLink>(context), ISubsCallLinkRepository { }
