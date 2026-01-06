using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Core;
using LeadPipe.Infrastructure.Interfaces.Service;
using LeadPipe.Infrastructure.Settings;
using Microsoft.Extensions.Logging;

namespace LeadPipe.Infrastructure.Data.Source;

internal class LeasedFileDataSource(IInfrastructureSettings settings, ICsvRwService csv, IJsonRwService json, ILogger<LeasedFileDataSource> logging)
    : FileDataSource<LeasedDto, LeasedFileDataSource>(new FileInfo(settings.LeasedSourceLoc!), csv, json, logging), IDataSourceAsync<LeasedDto>
{ }
