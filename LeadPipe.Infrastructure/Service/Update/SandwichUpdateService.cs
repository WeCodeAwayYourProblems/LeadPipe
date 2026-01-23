using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Service.Update;

internal sealed class SandwichUpdateService(
    IDataSourceAsync<SandMySqlEntity> sand,
    IEntityToVo<SandMySqlEntity, Sandwich> eToVo,
    IDataPersistence<Sandwich> persist
    ) : ValueObjectUpdateService<SandMySqlEntity, Sandwich>(sand, eToVo, persist), IUpdateService<Sandwich>
{ }
