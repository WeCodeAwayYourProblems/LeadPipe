using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository;
using LeadPipe.Infrastructure.Sqlite.Context;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class PlumbingCallLinkRepository(PlumbingContext context) : PlumbingContextRepository<PlumbingCallLink>(context), IPlumbingCallLinkRepository { }
