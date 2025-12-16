using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Settings;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Data.Source;

internal class LibacionFileDataSource(IInfrastructureSettings settings, ICsvRwService csv, IJsonRwService json, ILogger<LibacionFileDataSource> logging)
    : FileDataSource<LibacionDto, LibacionFileDataSource>(new FileInfo(settings.LibacionSourceLoc!), csv, json, logging), IDataSourceAsync<LibacionDto>
{ }
