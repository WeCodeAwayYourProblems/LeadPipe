using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Translate;

namespace LeadPipe.Infrastructure.Service;

[SourceKey(Source.Calli)]
internal sealed class CalliUpdateFromFileService(
    IDataSourceAsync<CalliDto> source,
    IDtoToVo<CalliDto, Plumbing> dtoToVo,
    IVoToEntity<Plumbing, PlumbingEntity> voToEntity,
    IDataPersistence<PlumbingEntity> persistence
    ) : UpdateService<CalliDto, Plumbing, PlumbingEntity>(source, dtoToVo, voToEntity, persistence), IUpdateService<Plumbing>
{ }
