using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Attributes;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Service.Update;

[SourceKey(Source.Lather)]
internal sealed class LatherUpdateService(
    IDataSourceAsync<LatherDto> source,
    IDtoToVo<LatherDto, Plumbing> dtoToVo,
    IVoToEntity<Plumbing, PlumbingEntity> voToEntity,
    IDataPersistence<PlumbingEntity> persistence
) : UpdateService<LatherDto, Plumbing, PlumbingEntity>(source, dtoToVo, voToEntity, persistence, SyncKey.Plumbing), IUpdateService<Plumbing>
{ }