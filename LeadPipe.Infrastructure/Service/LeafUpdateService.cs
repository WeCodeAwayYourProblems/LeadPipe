using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Service;

[SourceKey(Source.Leaf)]
internal sealed class LeafUpdateService(
    IDataSourceAsync<LeafDto> source,
    IDtoToVo<LeafDto, Plumbing> dtoToVo,
    IVoToEntity<Plumbing, PlumbingEntity> voToEntity,
    IDataPersistence<PlumbingEntity> persistence
    ) : UpdateService<LeafDto, Plumbing, PlumbingEntity>(source, dtoToVo, voToEntity, persistence), IUpdateService<Plumbing>
{ }
