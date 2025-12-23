using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Service;

internal sealed class SandwichUpdateService(
    IDataSourceAsync<SubMySqlEntity> subs,
    IEntityToVo<SubMySqlEntity, Sandwich> eToVo,
    IDataPersistence<Sandwich> persist
    ) : ValueObjectUpdateService<SubMySqlEntity, Sandwich>(subs, eToVo, persist), IUpdateService<Sandwich>
{ }