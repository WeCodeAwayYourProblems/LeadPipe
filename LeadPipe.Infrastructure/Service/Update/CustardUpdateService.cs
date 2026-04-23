using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Service.Update;

internal sealed class CustardUpdateService(
    IDataSourceAsync<CustardMySqlEntity> custard,
    IEntityToVo<CustardMySqlEntity, Custard> eToVo,
    IDataPersistence<Custard> persist
    ) : ValueObjectUpdateService<CustardMySqlEntity, Custard>(custard, eToVo, persist, SyncKey.Custard), IUpdateService<Custard>
{ }