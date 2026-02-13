using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Entity.MySql;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Service.Update;

internal sealed class CornUpdateService(
    IDataSourceAsync<CornMySqlEntity> corn,
    IEntityToVo<CornMySqlEntity, CornFormula> eToVo,
    IDataPersistence<CornFormula> persist
    ) : ValueObjectUpdateService<CornMySqlEntity, CornFormula>(corn, eToVo, persist), IUpdateService<CornFormula>
{ }