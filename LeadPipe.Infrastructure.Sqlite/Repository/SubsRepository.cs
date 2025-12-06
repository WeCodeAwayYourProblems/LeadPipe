using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Repository;
using LeadPipe.Infrastructure.Sqlite.Context;

namespace LeadPipe.Infrastructure.Sqlite.Repository;

public class SubsRepository(PlumbingContext context) : PlumbingContextRepository<SubsEntity>(context), ISubsRepository { }
