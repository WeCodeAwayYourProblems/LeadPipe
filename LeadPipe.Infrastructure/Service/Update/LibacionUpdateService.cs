using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Attributes;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Service.Update;

[SourceKey(Source.Libacion)]
internal sealed class LibacionUpdateService(
    IDataSourceAsync<LibacionDto> source,
    IDtoToVo<LibacionDto, Plumbing> dtoToVo,
    IVoToEntity<Plumbing, PlumbingEntity> voToEntity,
    IDataPersistence<PlumbingEntity> persistence
    ) : UpdateService<LibacionDto, Plumbing, PlumbingEntity>(source, dtoToVo, voToEntity, persistence, SyncKey.Plumbing), IUpdateService<Plumbing>
{ }