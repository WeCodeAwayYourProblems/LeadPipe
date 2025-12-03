using LeadPipe.Application.Service;
using LeadPipe.Domain.ValueObjects;
using LeadPipe.Infrastructure.Data.Persistence;
using LeadPipe.Infrastructure.Data.Source;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Entity.Sqlite;
using LeadPipe.Infrastructure.Translate;

namespace LeadPipe.Infrastructure.Service;

[SourceKey(Source.Lab)]
internal class LabUpdateService(
    IDataSourceAsync<LabDto> source, 
    IDtoToVo<LabDto, Plumbing> dtoToVo, 
    IVoToEntity<Plumbing, PlumbingEntity> voToEntity, 
    IDataPersistence<PlumbingEntity> persistence
    ) : UpdateService<LabDto, Plumbing, PlumbingEntity>(source, dtoToVo, voToEntity, persistence), IUpdateService<Plumbing> { }
