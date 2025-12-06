using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository;
using LeadPipe.Infrastructure.Sqlite.Context;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public class CallRepository(PlumbingContext context) : PlumbingContextRepository<CallEntity>(context), ICallRepository { }
