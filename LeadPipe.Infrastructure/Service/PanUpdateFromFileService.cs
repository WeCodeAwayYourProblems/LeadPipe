using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Service;

[SourceKey(Source.Pan)]
internal sealed class PanUpdateFromFileService(
    IDataSourceAsync<PanDto> source,
    IDtoToVo<PanDto, Plumbing> dtoToVo,
    IVoToEntity<Plumbing, PlumbingEntity> voToEntity,
    IDataPersistence<PlumbingEntity> persistence
    ) : UpdateService<PanDto, Plumbing, PlumbingEntity>(source, dtoToVo, voToEntity, persistence), IUpdateService<Plumbing>
{ }
