using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Data.Persistence;
using LeadPipe.Infrastructure.Data.Source;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Translate;

namespace LeadPipe.Infrastructure.Service;

[SourceKey(Source.Yeller)]
internal sealed class YellerUpdateService(
    IDataSourceAsync<YellerDto> source,
    IDtoToVo<YellerDto, Plumbing> dtoToVo,
    IVoToEntity<Plumbing, PlumbingEntity> voToEntity,
    IDataPersistence<PlumbingEntity> persistence
    ) : UpdateService<YellerDto, Plumbing, PlumbingEntity>(source, dtoToVo, voToEntity, persistence), IUpdateService<Plumbing>
{ }