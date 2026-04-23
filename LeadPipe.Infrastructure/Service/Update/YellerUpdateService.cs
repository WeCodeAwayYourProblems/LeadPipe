using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Attributes;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Service.Update;

[SourceKey(Source.Yeller)]
internal sealed class YellerUpdateService(
    IDataSourceAsync<YellerDto> source,
    IDtoToVo<YellerDto, Plumbing> dtoToVo,
    IVoToEntity<Plumbing, PlumbingEntity> voToEntity,
    IDataPersistence<PlumbingEntity> persistence
    ) : UpdateService<YellerDto, Plumbing, PlumbingEntity>(source, dtoToVo, voToEntity, persistence, SyncKey.Plumbing), IUpdateService<Plumbing>
{ }
