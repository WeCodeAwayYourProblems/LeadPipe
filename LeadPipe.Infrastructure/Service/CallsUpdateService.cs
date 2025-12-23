using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Service;

internal sealed class CallsUpdateService(
    IDataSourceAsync<CallMySqlEntity> call,
    IEntityToVo<CallMySqlEntity, Call> eToVo,
    IDataPersistence<Call> persistence
    ) : ValueObjectUpdateService<CallMySqlEntity, Call>(call, eToVo, persistence), IUpdateService<Call>
{ }

