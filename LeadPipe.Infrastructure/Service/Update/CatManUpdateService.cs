using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Attributes;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Service.Update;

[SourceKey(Source.Yeller)]
internal sealed class CatManUpdateService(
    IDataSourceAsync<CatManDto> source,
    IDtoToVo<CatManDto, CornFormula> dtoToVo,
    IVoToEntity<CornFormula, CornEntity> voToEntity,
    IDataPersistence<CornEntity> persistence
    ) : UpdateService<CatManDto, CornFormula, CornEntity>(source, dtoToVo, voToEntity, persistence, SyncKey.CornFormula), IUpdateService<CornFormula>
{ }