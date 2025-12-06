using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Repository;
using LeadPipe.Infrastructure.Sqlite.Context;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public sealed class PlumbingCallLinkRepository(PlumbingContext context) : PlumbingContextRepository<PlumbingCallLink>(context), IPlumbingCallLinkRepository { }
