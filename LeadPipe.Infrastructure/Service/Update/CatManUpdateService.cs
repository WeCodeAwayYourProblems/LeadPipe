using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Service.Update;

internal sealed class CatManUpdateService(
    IDataSourceAsync<CatManDto> source,
    IDtoToVo<CatManDto, Caliper> dtoToVo,
    IVoToEntity<Caliper, CaliperEntity> voToEntity,
    IDataPersistence<CaliperEntity> persistence
    ) : UpdateService<CatManDto, Caliper, CaliperEntity>(source, dtoToVo, voToEntity, persistence), IUpdateService<Caliper>
{ }